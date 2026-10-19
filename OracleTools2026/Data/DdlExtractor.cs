using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace OracleTools2026.Data;

public sealed partial class DdlExtractor(IOracleSession session)
{
    private sealed record ObjectInfo(string Type, DateTime? CreatedAt, DateTime? LastDdlAt, string Status);

    public async Task<IReadOnlyList<DdlExtractionResult>> ExtractManyAsync(
        IReadOnlyList<string> qualifiedNames,
        DdlExtractionOptions options,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DdlExtractionResult>(qualifiedNames.Count);
        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        for (var i = 0; i < qualifiedNames.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ProgressInfo(i + 1, qualifiedNames.Count, $"Extraindo DDL do objeto {qualifiedNames[i]}"));
            results.Add(await ExtractAsync(connection, qualifiedNames[i], options, cancellationToken));
        }

        return results;
    }

    public async Task<DdlExtractionResult> ExtractAsync(string qualifiedName, DdlExtractionOptions options, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await ExtractAsync(connection, qualifiedName, options, cancellationToken);
    }

    private async Task<DdlExtractionResult> ExtractAsync(OracleConnection connection, string qualifiedName, DdlExtractionOptions options, CancellationToken cancellationToken)
    {
        var parts = qualifiedName.Replace("\"", "").Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return DdlExtractionResult.Failure(qualifiedName, $"Nome de objeto inválido: {qualifiedName}");
        }

        var owner = parts[0].ToUpperInvariant();
        var name = parts[1].ToUpperInvariant();
        var normalizedName = $"{owner}.{name}";

        try
        {
            var info = await GetObjectInfoAsync(connection, owner, name, cancellationToken);
            if (info is null)
            {
                return DdlExtractionResult.Failure(normalizedName, $"Objeto {normalizedName} não encontrado");
            }

            var source = info.Type switch
            {
                "FUNCTION" or "PROCEDURE" or "TRIGGER" or "PACKAGE" or "PACKAGE BODY" or "TYPE" or "TYPE BODY" or "JAVA SOURCE" =>
                    await ExtractStoredSourceAsync(connection, owner, name, info.Type, options.InlineCompileErrors, cancellationToken),
                "VIEW" => await ExtractViewAsync(connection, owner, name, cancellationToken),
                "SEQUENCE" => await ExtractSequenceAsync(connection, owner, name, cancellationToken),
                "JOB" => await ExtractJobAsync(connection, owner, name, cancellationToken),
                _ => null
            };

            if (source is null)
            {
                return DdlExtractionResult.Failure(normalizedName, $"Tipo de objeto {info.Type} não suportado", info.Type);
            }

            if (source.Length == 0)
            {
                return DdlExtractionResult.Failure(normalizedName, $"DDL não encontrado para o objeto {normalizedName}", info.Type);
            }

            var ddl = new StringBuilder(source);
            if (info.Type is not "JOB")
            {
                ddl.Append(await ExtractSynonymsAsync(connection, owner, name, cancellationToken));
            }

            if (info.Type is not ("JOB" or "JAVA SOURCE"))
            {
                ddl.Append(await ExtractGrantsAsync(connection, owner, name, cancellationToken));
            }

            var body = ddl.ToString().TrimEnd('\r', '\n');
            if (options.IncludeHeader)
            {
                body = await BuildHeaderAsync(connection, owner, name, info, cancellationToken) + "\n" + body;
            }

            var objectType = info.Type switch
            {
                "PACKAGE BODY" => "PACKAGE",
                "TYPE BODY" => "TYPE",
                var other => other
            };

            return new DdlExtractionResult(normalizedName, objectType, body.ReplaceLineEndings("\r\n"), null);
        }
        catch (OracleException ex)
        {
            return DdlExtractionResult.Failure(normalizedName, ex.Message);
        }
    }

    private static async Task<ObjectInfo?> GetObjectInfoAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT MIN(OBJECT_TYPE), MIN(CREATED), MAX(LAST_DDL_TIME), MIN(STATUS)
            FROM ALL_OBJECTS
            WHERE OWNER = :owner
              AND OBJECT_NAME = :objectName
              AND OBJECT_TYPE NOT LIKE '%TABLE%'
              AND OBJECT_TYPE <> 'SYNONYM'
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || reader.IsDBNull(0))
        {
            return null;
        }

        return new ObjectInfo(
            reader.GetString(0).Trim().ToUpperInvariant(),
            reader.GetDateTimeOrNull(1),
            reader.GetDateTimeOrNull(2),
            reader.GetStringOrEmpty(3));
    }

    private async Task<string> BuildHeaderAsync(OracleConnection connection, string owner, string name, ObjectInfo info, CancellationToken cancellationToken)
    {
        var serverInfo = await DatabaseObjectRepository.GetServerInfoAsync(connection, session.Current!.DataSource, cancellationToken);

        var header = new StringBuilder()
            .Append("-- Fonte extraído pelo Oracle Tools\n")
            .Append($"-- Dados de conexão: {serverInfo}\n")
            .Append($"-- Objeto criado em: {info.CreatedAt:dd/MM/yy HH:mm:ss}\n")
            .Append($"-- Última compilação deste objeto: {info.LastDdlAt:dd/MM/yy HH:mm:ss}\n")
            .Append($"-- Status deste objeto: {info.Status}\n");

        var errors = await DatabaseObjectRepository.GetCompileErrorsAsync(connection, (owner, name), cancellationToken);
        var messageLines = errors.Values.SelectMany(m => m.Split("\r\n")).Where(l => l.Length > 0).ToList();
        if (messageLines.Count > 0)
        {
            header.Append("-- Mensagem:\n");
            foreach (var line in messageLines)
            {
                header.Append($"--   {line}\n");
            }
        }

        if (await session.HasAuditTablesAsync(connection, cancellationToken))
        {
            header.Append(await BuildAuditHeaderAsync(connection, owner, name, cancellationToken));
        }

        return header.ToString();
    }

    private static async Task<string> BuildAuditHeaderAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT MAX(AU.DATA_HORA) AS DATA_HORA,
                   CO.NOME_COMPLETO,
                   CO.EMAIL,
                   AU.OSUSER,
                   AU.CURRENT_USER,
                   AU.HOST
            FROM AESPROD.AUDITORIA_OBJETOS AU
                LEFT JOIN AESPROD.CONTATO_CONTROLE_CONCORRENCIA CO
                ON AU.ID_USUARIO = CO.ID
            WHERE AU.OWNER = :owner
              AND AU.NOME = :objectName
            GROUP BY AU.OWNER, AU.NOME, CO.ID, CO.NOME_COMPLETO, CO.EMAIL, AU.OSUSER, AU.CURRENT_USER, AU.HOST
            ORDER BY MAX(AU.DATA_HORA) DESC
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return "";
        }

        var analyst = reader.IsDBNull(1) ? "INDEFINIDO" : reader.GetString(1);
        var email = reader.GetStringOrEmpty(2).Trim();

        return new StringBuilder()
            .Append($"-- Último CREATE OR REPLACE ou DEBUG: {reader.GetDateTimeOrNull(0):dd/MM/yy HH:mm:ss}\n")
            .Append($"-- Analista: {analyst}{(email.Length > 0 ? $" <{email}>" : "")}\n")
            .Append($"-- Usuário de banco: {reader.GetStringOrEmpty(4)}\n")
            .Append($"-- Nome da máquina: {reader.GetStringOrEmpty(5)}\n")
            .Append($"-- Usuário da máquina: {reader.GetStringOrEmpty(3)}\n")
            .ToString();
    }

    private static async Task<string> ExtractStoredSourceAsync(
        OracleConnection connection, string owner, string name, string objectType, bool inlineErrors, CancellationToken cancellationToken)
    {
        var (specType, bodyType) = objectType switch
        {
            "PACKAGE" or "PACKAGE BODY" => ("PACKAGE", "PACKAGE BODY"),
            "TYPE" or "TYPE BODY" => ("TYPE", "TYPE BODY"),
            var other => (other, (string?)null)
        };

        var ddl = new StringBuilder();
        var spec = await ReadSourceAsync(connection, owner, name, specType, inlineErrors, cancellationToken);
        if (spec.Length == 0)
        {
            return "";
        }

        if (specType == "TRIGGER" && await IsTriggerDisabledAsync(connection, owner, name, cancellationToken))
        {
            spec = InsertDisableClause(spec);
        }

        ddl.Append(spec);

        if (bodyType is not null)
        {
            ddl.Append(await ReadSourceAsync(connection, owner, name, bodyType, inlineErrors, cancellationToken));
        }

        return ddl.ToString();
    }

    private static async Task<string> ReadSourceAsync(
        OracleConnection connection, string owner, string name, string sourceType, bool inlineErrors, CancellationToken cancellationToken)
    {
        var errorsByLine = inlineErrors
            ? await ReadErrorsByLineAsync(connection, owner, name, sourceType, cancellationToken)
            : new Dictionary<int, string>();

        const string sql = """
            SELECT TEXT, LINE
            FROM ALL_SOURCE
            WHERE OWNER = :owner
              AND NAME = :objectName
              AND TYPE = :sourceType
            ORDER BY LINE
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name), ("sourceType", sourceType));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var source = new StringBuilder();
        var isFirstLine = true;
        while (await reader.ReadAsync(cancellationToken))
        {
            var line = reader.GetStringOrEmpty(0);
            var lineNumber = reader.GetInt32(1);

            if (isFirstLine)
            {
                source.Append("CREATE OR REPLACE ");
                if (sourceType == "JAVA SOURCE")
                {
                    source.Append($"AND COMPILE JAVA SOURCE NAMED {owner}.{name} AS\n");
                }
                else
                {
                    line = NormalizeFirstLine(line, owner, name) + "\n";
                }

                isFirstLine = false;
            }

            if (errorsByLine.TryGetValue(lineNumber, out var error))
            {
                line = line.TrimEnd('\r', '\n') + " -- ###ERRO### " + error + "\n";
            }

            source.Append(line);
        }

        if (source.Length == 0)
        {
            return "";
        }

        if (source[^1] is not ('\n' or '\r'))
        {
            source.Append('\n');
        }

        return source.Append("/\n\n").ToString();
    }

    private static string NormalizeFirstLine(string line, string owner, string name)
    {
        line = line.Trim().ToUpperInvariant().Replace("\"", "");
        if (!line.Contains(owner + ".", StringComparison.Ordinal))
        {
            line = Regex.Replace(line, $@"\b{Regex.Escape(name)}\b", $"{owner}.{name}");
        }

        return MultipleSpacesRegex().Replace(line, " ");
    }

    private static async Task<Dictionary<int, string>> ReadErrorsByLineAsync(
        OracleConnection connection, string owner, string name, string sourceType, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT LINE, TEXT
            FROM ALL_ERRORS
            WHERE OWNER = :owner
              AND NAME = :objectName
              AND TYPE = :sourceType
            ORDER BY LINE, SEQUENCE
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name), ("sourceType", sourceType));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var errors = new Dictionary<int, string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var message = MultipleSpacesRegex().Replace(reader.GetStringOrEmpty(1).Replace("\r", ""), " ").Replace("\n", "/").Trim();
            if (message.Length == 0)
            {
                continue;
            }

            var line = reader.GetInt32(0);
            errors[line] = errors.TryGetValue(line, out var existing) ? $"{existing} / {message}" : message;
        }

        return errors;
    }

    private static async Task<bool> IsTriggerDisabledAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = "SELECT STATUS FROM ALL_TRIGGERS WHERE OWNER = :owner AND TRIGGER_NAME = :objectName";
        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        var status = await command.ExecuteScalarAsync(cancellationToken) as string;
        return string.Equals(status?.Trim(), "DISABLED", StringComparison.OrdinalIgnoreCase);
    }

    private static string InsertDisableClause(string triggerSource)
    {
        var lines = triggerSource.Split('\n');
        var index = Array.FindIndex(lines, l => BeginOrDeclareRegex().IsMatch(l));
        if (index < 0)
        {
            return triggerSource;
        }

        return string.Join('\n', lines.Take(index).Append("DISABLE").Concat(lines.Skip(index)));
    }

    private static async Task<string> ExtractViewAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string columnsSql = """
            SELECT COLUMN_NAME
            FROM ALL_TAB_COLUMNS
            WHERE OWNER = :owner
              AND TABLE_NAME = :objectName
            ORDER BY COLUMN_ID
            """;

        var columns = new List<string>();
        await using (var command = connection.CreateTextCommand(columnsSql, ("owner", owner), ("objectName", name)))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(0));
            }
        }

        const string viewSql = "SELECT TEXT FROM ALL_VIEWS WHERE OWNER = :owner AND VIEW_NAME = :objectName";
        await using var viewCommand = connection.CreateTextCommand(viewSql, ("owner", owner), ("objectName", name));
        viewCommand.InitialLONGFetchSize = -1;
        await using var viewReader = await viewCommand.ExecuteReaderAsync(cancellationToken);
        if (!await viewReader.ReadAsync(cancellationToken))
        {
            return "";
        }

        var text = viewReader.GetStringOrEmpty(0).TrimEnd();
        return new StringBuilder()
            .Append($"CREATE OR REPLACE VIEW {owner}.{name}\n(\n")
            .Append(string.Join(",\n", columns.Select(c => "   " + c)))
            .Append("\n)\nAS\n")
            .Append(text)
            .Append("\n/\n\n")
            .ToString();
    }

    private static async Task<string> ExtractSequenceAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT LAST_NUMBER, INCREMENT_BY, MAX_VALUE, MIN_VALUE, CYCLE_FLAG, CACHE_SIZE, ORDER_FLAG
            FROM ALL_SEQUENCES
            WHERE SEQUENCE_OWNER = :owner
              AND SEQUENCE_NAME = :objectName
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return "";
        }

        var cacheSize = reader.GetNumberText(5);
        return new StringBuilder()
            .Append($"CREATE SEQUENCE {owner}.{name}\n")
            .Append($"  START WITH {reader.GetNumberText(0)}\n")
            .Append($"  INCREMENT BY {reader.GetNumberText(1)}\n")
            .Append($"  MAXVALUE {reader.GetNumberText(2)}\n")
            .Append($"  MINVALUE {reader.GetNumberText(3)}\n")
            .Append(reader.GetStringOrEmpty(4).Trim() == "N" ? "  NOCYCLE\n" : "  CYCLE\n")
            .Append(cacheSize is "0" or "" ? "  NOCACHE\n" : $"  CACHE {cacheSize}\n")
            .Append(reader.GetStringOrEmpty(6).Trim() == "N" ? "  NOORDER\n" : "  ORDER\n")
            .Append("/\n\n")
            .ToString();
    }

    private static async Task<string> ExtractJobAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT OWNER, JOB_NAME, PROGRAM_OWNER, PROGRAM_NAME, JOB_TYPE, JOB_ACTION,
                   SCHEDULE_OWNER, SCHEDULE_NAME, REPEAT_INTERVAL, ENABLED, COMMENTS
            FROM ALL_SCHEDULER_JOBS
            WHERE OWNER = :owner
              AND JOB_NAME = :objectName
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return "";
        }

        static string Escape(string value) => value.Replace("'", "''");

        var job = new StringBuilder()
            .Append("BEGIN\n")
            .Append("   DBMS_SCHEDULER.CREATE_JOB (\n")
            .Append($"   job_name           =>  '{reader.GetString(0)}.{reader.GetString(1)}',\n");

        if (reader.IsDBNull(4))
        {
            job.Append($"   program_name       =>  '{reader.GetStringOrEmpty(2)}.{reader.GetStringOrEmpty(3)}',\n");
        }
        else
        {
            job.Append($"   job_type           =>  '{reader.GetString(4)}',\n");
            job.Append($"   job_action         =>  '{Escape(reader.GetStringOrEmpty(5))}',\n");
        }

        if (reader.IsDBNull(8))
        {
            if (!reader.IsDBNull(7))
            {
                job.Append($"   schedule_name      =>  '{reader.GetStringOrEmpty(6)}.{reader.GetString(7)}',\n");
            }
        }
        else
        {
            job.Append("   start_date         =>  SYSDATE,\n");
            job.Append($"   repeat_interval    =>  '{Escape(reader.GetString(8))}',\n");
            job.Append("   end_date           =>  NULL,\n");
        }

        job.Append($"   enabled            =>  {reader.GetStringOrEmpty(9)},\n");
        job.Append($"   comments           =>  '{Escape(reader.GetStringOrEmpty(10))}');\n");
        job.Append("END;\n/\n\n");
        return job.ToString();
    }

    private static async Task<string> ExtractSynonymsAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT OWNER, SYNONYM_NAME
            FROM ALL_SYNONYMS
            WHERE TABLE_OWNER = :owner
              AND TABLE_NAME = :objectName
            ORDER BY OWNER, SYNONYM_NAME
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var synonyms = new StringBuilder();
        while (await reader.ReadAsync(cancellationToken))
        {
            var synonymOwner = reader.GetString(0);
            var synonymName = reader.GetString(1);
            synonyms.Append(synonymOwner == "PUBLIC"
                ? $"CREATE OR REPLACE PUBLIC SYNONYM {synonymName} FOR {owner}.{name}\n/\n\n"
                : $"CREATE OR REPLACE SYNONYM {synonymOwner}.{synonymName} FOR {owner}.{name}\n/\n\n");
        }

        return synonyms.ToString();
    }

    private static async Task<string> ExtractGrantsAsync(OracleConnection connection, string owner, string name, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT GRANTEE, PRIVILEGE, GRANTABLE
            FROM ALL_TAB_PRIVS
            WHERE TABLE_SCHEMA = :owner
              AND TABLE_NAME = :objectName
            ORDER BY GRANTEE, PRIVILEGE
            """;

        await using var command = connection.CreateTextCommand(sql, ("owner", owner), ("objectName", name));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var privileges = new List<(string Grantee, string Privilege, bool Grantable)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            privileges.Add((reader.GetString(0), reader.GetString(1), reader.GetStringOrEmpty(2) == "YES"));
        }

        var grants = new StringBuilder();
        foreach (var byGrantee in privileges.GroupBy(p => p.Grantee))
        {
            foreach (var group in byGrantee.GroupBy(p => p.Grantable).OrderBy(g => g.Key))
            {
                var list = string.Join(", ", group.Select(p => p.Privilege).Distinct());
                var suffix = group.Key ? " WITH GRANT OPTION" : "";
                grants.Append($"GRANT {list} ON {owner}.{name} TO {byGrantee.Key}{suffix}\n/\n\n");
            }
        }

        return grants.ToString();
    }

    [GeneratedRegex(" {2,}")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@"\b(BEGIN|DECLARE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex BeginOrDeclareRegex();
}
