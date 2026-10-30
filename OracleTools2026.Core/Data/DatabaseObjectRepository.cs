using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Runtime.InteropServices;
using System.Text;

namespace OracleTools2026.Data;

public sealed class DatabaseObjectRepository(IOracleSession session)
{
    public static readonly IReadOnlyList<string> SupportedTypes =
        ["FUNCTION", "JOB", "PACKAGE", "PROCEDURE", "SEQUENCE", "TRIGGER", "TYPE", "VIEW", "JAVA SOURCE"];

    private const string NormalizedType = """
        CASE OBJECT_TYPE WHEN 'PACKAGE BODY' THEN 'PACKAGE' WHEN 'TYPE BODY' THEN 'TYPE' ELSE OBJECT_TYPE END
        """;

    private const string RecentlyCompiledExcludedTypes =
        "'TABLE', 'TABLE PARTITION', 'TABLE SUBPARTITION', 'SYNONYM', 'INDEX', 'INDEX PARTITION', " +
        "'LOB PARTITION', 'DIRECTORY', 'QUEUE', 'TYPE', 'TYPE BODY', 'LOB'";

    public async Task<string> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetServerInfoAsync(connection, session.Current!.DataSourceDisplay, cancellationToken);
    }

    internal static async Task<string> GetServerInfoAsync(OracleConnection connection, string dataSource, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TO_CHAR(SYSDATE, 'DD/MM/YY HH24:MI:SS'),
                   UPPER(SYS_CONTEXT('USERENV', 'SERVER_HOST')),
                   UPPER(SYS_CONTEXT('USERENV', 'INSTANCE_NAME')),
                   UPPER(SYS_CONTEXT('USERENV', 'CURRENT_USER'))
            FROM DUAL
            """;

        await using var command = connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return "";
        }

        return $"{reader.GetString(0)} - Host: {reader.GetStringOrEmpty(1)} - Instance: {reader.GetStringOrEmpty(2)} " +
               $"- User: {reader.GetStringOrEmpty(3)} - Alias: {dataSource}";
    }

    public async Task<IReadOnlyList<DatabaseProperty>> GetDatabaseDetailsAsync(CancellationToken cancellationToken = default)
    {
        const string databaseGroup = "Banco de dados";
        const string instanceGroup = "Instância";
        const string storageGroup = "Armazenamento";
        const string nlsGroup = "NLS";
        const string sessionGroup = "Sessão";
        const string clientGroup = "Cliente";

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        var properties = new List<DatabaseProperty>();

        void Add(string group, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                properties.Add(new DatabaseProperty(group, name, value.Trim()));
            }
        }

        // VERSION_FULL e CON_NAME só existem a partir do 18c/12c; em versões anteriores a consulta falha e cai no fallback.
        var version = await TryReadRowAsync(connection, "SELECT VERSION_FULL FROM PRODUCT_COMPONENT_VERSION WHERE PRODUCT LIKE 'Oracle%' AND ROWNUM = 1", cancellationToken)
            ?? await TryReadRowAsync(connection, "SELECT VERSION FROM PRODUCT_COMPONENT_VERSION WHERE PRODUCT LIKE 'Oracle%' AND ROWNUM = 1", cancellationToken);
        Add(databaseGroup, "Versão", version?[0]);

        var banner = await TryReadRowAsync(connection, "SELECT BANNER FROM V$VERSION WHERE BANNER LIKE 'Oracle%' AND ROWNUM = 1", cancellationToken);
        Add(databaseGroup, "Produto", banner?[0]);

        const string contextSql = """
            SELECT SYS_CONTEXT('USERENV', 'DB_NAME'),
                   SYS_CONTEXT('USERENV', 'DB_UNIQUE_NAME'),
                   SYS_CONTEXT('USERENV', 'SERVICE_NAME'),
                   SYS_CONTEXT('USERENV', 'INSTANCE_NAME'),
                   SYS_CONTEXT('USERENV', 'SERVER_HOST'),
                   DBTIMEZONE,
                   TO_CHAR(SYSTIMESTAMP, 'DD/MM/YYYY HH24:MI:SS TZH:TZM')
            FROM DUAL
            """;
        if (await TryReadRowAsync(connection, contextSql, cancellationToken) is { } context)
        {
            Add(databaseGroup, "Nome", context[0]);
            Add(databaseGroup, "Nome único", context[1]);
            Add(databaseGroup, "Serviço", context[2]);
            Add(databaseGroup, "Instância", context[3]);
            Add(databaseGroup, "Servidor", context[4]);
            Add(databaseGroup, "Fuso horário", context[5]);
            Add(databaseGroup, "Data/hora do servidor", context[6]);
        }

        var container = await TryReadRowAsync(connection, "SELECT SYS_CONTEXT('USERENV', 'CON_NAME') FROM DUAL", cancellationToken);
        Add(databaseGroup, "Container (PDB)", container?[0]);

        const string databaseSql = """
            SELECT TO_CHAR(CREATED, 'DD/MM/YYYY HH24:MI:SS'), LOG_MODE, OPEN_MODE, DATABASE_ROLE, PLATFORM_NAME
            FROM V$DATABASE
            """;
        if (await TryReadRowAsync(connection, databaseSql, cancellationToken) is { } database)
        {
            Add(databaseGroup, "Criado em", database[0]);
            Add(databaseGroup, "Modo de log", database[1]);
            Add(databaseGroup, "Modo de abertura", database[2]);
            Add(databaseGroup, "Papel", database[3]);
            Add(databaseGroup, "Plataforma", database[4]);
        }

        const string instanceSql = """
            SELECT HOST_NAME,
                   STATUS,
                   TO_CHAR(STARTUP_TIME, 'DD/MM/YYYY HH24:MI:SS'),
                   TRUNC(SYSDATE - STARTUP_TIME) || ' dia(s) e ' || TRUNC(MOD((SYSDATE - STARTUP_TIME) * 24, 24)) || ' hora(s)'
            FROM V$INSTANCE
            """;
        if (await TryReadRowAsync(connection, instanceSql, cancellationToken) is { } instance)
        {
            Add(instanceGroup, "Host", instance[0]);
            Add(instanceGroup, "Status", instance[1]);
            Add(instanceGroup, "Iniciada em", instance[2]);
            Add(instanceGroup, "Tempo no ar", instance[3]);
        }

        const string storageSql = """
            SELECT TO_CHAR((SELECT SUM(BYTES) FROM DBA_DATA_FILES) / 1073741824, 'FM999G999G990D00', 'NLS_NUMERIC_CHARACTERS='',.''') || ' GB',
                   TO_CHAR((SELECT SUM(BYTES) FROM DBA_SEGMENTS) / 1073741824, 'FM999G999G990D00', 'NLS_NUMERIC_CHARACTERS='',.''') || ' GB'
            FROM DUAL
            """;
        if (await TryReadRowAsync(connection, storageSql, cancellationToken) is { } storage)
        {
            Add(storageGroup, "Alocado (datafiles)", storage[0]);
            Add(storageGroup, "Utilizado (segmentos)", storage[1]);
        }

        const string nlsSql = """
            SELECT MAX(CASE PARAMETER WHEN 'NLS_CHARACTERSET' THEN VALUE END),
                   MAX(CASE PARAMETER WHEN 'NLS_NCHAR_CHARACTERSET' THEN VALUE END),
                   MAX(CASE PARAMETER WHEN 'NLS_LANGUAGE' THEN VALUE END),
                   MAX(CASE PARAMETER WHEN 'NLS_TERRITORY' THEN VALUE END)
            FROM NLS_DATABASE_PARAMETERS
            """;
        if (await TryReadRowAsync(connection, nlsSql, cancellationToken) is { } nls)
        {
            Add(nlsGroup, "Character set", nls[0]);
            Add(nlsGroup, "National character set", nls[1]);
            Add(nlsGroup, "Idioma", nls[2]);
            Add(nlsGroup, "Território", nls[3]);
        }

        const string sessionSql = """
            SELECT SYS_CONTEXT('USERENV', 'SESSION_USER'),
                   SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA'),
                   SYS_CONTEXT('USERENV', 'SID'),
                   SYS_CONTEXT('USERENV', 'OS_USER'),
                   SYS_CONTEXT('USERENV', 'TERMINAL'),
                   SYS_CONTEXT('USERENV', 'IP_ADDRESS'),
                   SESSIONTIMEZONE
            FROM DUAL
            """;
        if (await TryReadRowAsync(connection, sessionSql, cancellationToken) is { } sessionInfo)
        {
            Add(sessionGroup, "Usuário", sessionInfo[0]);
            Add(sessionGroup, "Schema atual", sessionInfo[1]);
            Add(sessionGroup, "SID", sessionInfo[2]);
            Add(sessionGroup, "Usuário do SO", sessionInfo[3]);
            Add(sessionGroup, "Terminal", sessionInfo[4]);
            Add(sessionGroup, "Endereço IP", sessionInfo[5]);
            Add(sessionGroup, "Fuso horário", sessionInfo[6]);
        }

        Add(sessionGroup, "Banco", session.Current!.DataSourceDisplay);
        if (session.Current.Privilege != DbaPrivilege.Normal)
        {
            Add(sessionGroup, "Privilégio", ConnectionInfo.PrivilegeName(session.Current.Privilege));
        }

        Add(clientGroup, "Versão do servidor (driver)", connection.ServerVersion);
        Add(clientGroup, "Oracle.ManagedDataAccess", typeof(OracleConnection).Assembly.GetName().Version?.ToString());
        Add(clientGroup, "OracleTools2026", typeof(DatabaseObjectRepository).Assembly.GetName().Version?.ToString(3));
        Add(clientGroup, ".NET", RuntimeInformation.FrameworkDescription);

        return properties;
    }

    private static async Task<string[]?> TryReadRowAsync(OracleConnection connection, string sql, CancellationToken cancellationToken)
    {
        try
        {
            await using var command = connection.CreateTextCommand(sql);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            var values = new string[reader.FieldCount];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = reader.GetStringOrEmpty(i);
            }

            return values;
        }
        catch (OracleException)
        {
            // Sem privilégio na view (ex.: V$INSTANCE, DBA_*) ou recurso inexistente na versão do banco: a informação é omitida.
            return null;
        }
    }

    public async Task<IReadOnlyList<string>> GetOwnersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand("SELECT DISTINCT OWNER FROM ALL_OBJECTS ORDER BY OWNER");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var owners = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            owners.Add(reader.GetString(0));
        }

        return owners;
    }

    public async Task<IReadOnlyList<DatabaseObject>> SearchAsync(string? owner, string? type, string? name, CancellationToken cancellationToken = default)
    {
        var where = new StringBuilder($"OBJECT_TYPE IN ({ToSqlList(SupportedTypes.Concat(["PACKAGE BODY", "TYPE BODY"]))})");
        var parameters = new List<(string, object?)>();
        AppendFilters(where, parameters, owner, type, name);

        return await QueryObjectsAsync(where.ToString(), having: null, orderBy: "OWNER, OBJECT_TYPE, OBJECT_NAME", parameters, cancellationToken);
    }

    private static void AppendFilters(StringBuilder where, List<(string, object?)> parameters, string? owner, string? type, string? name)
    {
        if (!string.IsNullOrWhiteSpace(owner))
        {
            where.Append(" AND OWNER = :owner");
            parameters.Add(("owner", owner.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            where.Append($" AND {NormalizedType} = :objectType");
            parameters.Add(("objectType", type.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            where.Append(" AND UPPER(OBJECT_NAME) LIKE '%' || :objectName || '%'");
            parameters.Add(("objectName", name.Trim().ToUpperInvariant()));
        }
    }

    public async Task<IReadOnlyList<DatabaseObject>> GetRecentlyCompiledAsync(int days, CancellationToken cancellationToken = default)
    {
        var where = $"OBJECT_TYPE NOT IN ({RecentlyCompiledExcludedTypes})";
        const string having = "CASE WHEN MIN(OBJECT_TYPE) = 'JOB' THEN MIN(CREATED) ELSE MAX(LAST_DDL_TIME) END > SYSDATE - :days";

        return await QueryObjectsAsync(where, having, "LAST_DDL_TIME DESC, OWNER, OBJECT_NAME", [("days", days)], cancellationToken);
    }

    public async Task<IReadOnlyList<DatabaseObject>> GetInvalidAsync(string? owner, string? type, string? name, CancellationToken cancellationToken = default)
    {
        var where = new StringBuilder("1 = 1");
        var parameters = new List<(string, object?)>();
        AppendFilters(where, parameters, owner, type, name);
        const string having = "MAX(CASE WHEN STATUS <> 'VALID' THEN 1 ELSE 0 END) = 1";

        return await QueryObjectsAsync(where.ToString(), having, "LAST_DDL_TIME DESC, OWNER, OBJECT_NAME", parameters, cancellationToken);
    }

    private async Task<IReadOnlyList<DatabaseObject>> QueryObjectsAsync(
        string where,
        string? having,
        string orderBy,
        IReadOnlyList<(string Name, object? Value)> parameters,
        CancellationToken cancellationToken)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        var sql = $"""
            SELECT OWNER, OBJECT_NAME, OBJECT_TYPE, CREATED, LAST_DDL_TIME, STATUS
            FROM (
                SELECT OWNER,
                       OBJECT_NAME,
                       {NormalizedType} AS OBJECT_TYPE,
                       MIN(CREATED) AS CREATED,
                       MAX(LAST_DDL_TIME) AS LAST_DDL_TIME,
                       CASE WHEN MAX(CASE WHEN STATUS <> 'VALID' THEN 1 ELSE 0 END) = 1 THEN 'INVALID' ELSE 'VALID' END AS STATUS
                FROM ALL_OBJECTS
                WHERE {where}
                GROUP BY OWNER, OBJECT_NAME, {NormalizedType}
                {(having is null ? "" : "HAVING " + having)}
            )
            ORDER BY {orderBy}
            """;

        var objects = new List<DatabaseObject>();
        await using (var command = connection.CreateTextCommand(sql, [.. parameters]))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                objects.Add(new DatabaseObject
                {
                    Owner = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedAt = reader.GetDateTimeOrNull(3),
                    LastDdlAt = reader.GetDateTimeOrNull(4),
                    Status = reader.GetStringOrEmpty(5)
                });
            }
        }

        return await FillCompileErrorsAsync(connection, objects, cancellationToken);
    }

    private static async Task<IReadOnlyList<DatabaseObject>> FillCompileErrorsAsync(
        OracleConnection connection, List<DatabaseObject> objects, CancellationToken cancellationToken)
    {
        var invalid = objects.Where(o => o.Status == "INVALID").ToList();
        if (invalid.Count == 0)
        {
            return objects;
        }

        // Com um único objeto inválido filtra no banco; com vários, uma só consulta em ALL_ERRORS é mais barata que N consultas.
        (string Owner, string Name)? filter = invalid.Count == 1 ? (invalid[0].Owner, invalid[0].Name) : null;
        var errors = await GetCompileErrorsAsync(connection, filter, cancellationToken);
        if (errors.Count == 0)
        {
            return objects;
        }

        return objects
            .Select(o => o.Status == "INVALID" && errors.TryGetValue((o.Owner, o.Name, o.Type), out var message) ? o with { ErrorMessage = message } : o)
            .ToList();
    }

    internal static async Task<Dictionary<(string Owner, string Name, string Type), string>> GetCompileErrorsAsync(
        OracleConnection connection, (string Owner, string Name)? filter, CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT ERR.OWNER, ERR.NAME, ERR.TYPE, ERR.LINE, ERR.POSITION, ERR.TEXT, SOU.TEXT AS SOURCE_LINE
            FROM ALL_ERRORS ERR
                LEFT JOIN ALL_SOURCE SOU
                ON ERR.OWNER = SOU.OWNER
                AND ERR.NAME = SOU.NAME
                AND ERR.TYPE = SOU.TYPE
                AND ERR.LINE = SOU.LINE
            WHERE ERR.ATTRIBUTE = 'ERROR'
              AND UPPER(ERR.TEXT) NOT LIKE '%STATEMENT IGNORED%'
              {(filter is null ? "" : "AND ERR.OWNER = :owner AND ERR.NAME = :objectName")}
            ORDER BY ERR.OWNER, ERR.NAME, ERR.TYPE, ERR.SEQUENCE
            """;

        await using var command = filter is { } f
            ? connection.CreateTextCommand(sql, ("owner", f.Owner), ("objectName", f.Name))
            : connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var errors = new Dictionary<(string, string, string), StringBuilder>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var type = reader.GetString(2) switch
            {
                "PACKAGE BODY" => "PACKAGE",
                "TYPE BODY" => "TYPE",
                var other => other
            };
            var key = (reader.GetString(0), reader.GetString(1), type);
            if (!errors.TryGetValue(key, out var message))
            {
                message = new StringBuilder();
                errors[key] = message;
            }
            else
            {
                message.Append("\r\n");
            }

            var line = reader.GetNumberText(3);
            if (line is not ("" or "0"))
            {
                message.Append($"Nº LINHA:{line} - Nº COLUNA:{reader.GetNumberText(4)} - ");
            }

            message.Append("MENSAGEM: ").Append(reader.GetStringOrEmpty(5).ReplaceLineEndings(" "));

            var sourceLine = reader.GetStringOrEmpty(6).Trim();
            if (sourceLine.Length > 0)
            {
                message.Append($" - TEXTO DA LINHA {line}: {sourceLine}");
            }
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToString());
    }

    private static string ToSqlList(IEnumerable<string> values) =>
        string.Join(", ", values.Select(v => $"'{v.Replace("'", "''")}'"));
}
