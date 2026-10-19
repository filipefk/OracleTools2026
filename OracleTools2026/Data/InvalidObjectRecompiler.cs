using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Text;

namespace OracleTools2026.Data;

public sealed class InvalidObjectRecompiler(IOracleSession session)
{
    private const int MaxAttempts = 10;

    private sealed record CompileStatement(string Description, string Sql);

    public async Task<RecompileResult> RecompileAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        var report = new StringBuilder();
        var anyRecompiled = false;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            void Log(string message) => report.Append($"Tentativa: {attempt} - {message}\r\n");

            Log("Buscando quantidade de objetos inválidos antes");
            var before = await CountInvalidAsync(connection, cancellationToken);
            if (before == 0)
            {
                Log("Nenhum objeto inválido encontrado, encerrando execução");
                break;
            }

            var statements = await GetCompileStatementsAsync(connection, cancellationToken);
            for (var i = 0; i < statements.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var statement = statements[i];
                progress?.Report(new ProgressInfo(i + 1, statements.Count, $"Tentativa {attempt} - Recompilando {statement.Description}"));
                Log($"Recompilando {statement.Description}");

                try
                {
                    await using var command = connection.CreateTextCommand(statement.Sql);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (OracleException ex)
                {
                    report.Append($"ERRO: {ex.Message}\r\n");
                }
            }

            Log("Buscando quantidade de objetos inválidos depois");
            var after = await CountInvalidAsync(connection, cancellationToken);
            Log($"Objetos inválidos antes = {before} objetos inválidos depois = {after}");

            anyRecompiled |= after < before;

            if (after == 0)
            {
                Log("Nenhum objeto inválido encontrado, encerrando execução");
                break;
            }

            if (after == before)
            {
                Log("Objetos inválidos antes igual a quantidade de objetos inválidos depois, encerrando execução");
                break;
            }

            Log(attempt < MaxAttempts
                ? "Objetos inválidos antes diferente da quantidade de objetos inválidos depois, executando mais uma vez"
                : "Limite de tentativas atingido, encerrando execução");
        }

        return new RecompileResult(anyRecompiled, report.ToString());
    }

    private static async Task<int> CountInvalidAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        var ignoredOwners = string.Join(", ", DatabaseObjectRepository.IgnoredInvalidSynonymOwners.Select(o => $"'{o}'"));
        var sql = $"""
            SELECT COUNT(*)
            FROM ALL_OBJECTS
            WHERE STATUS <> 'VALID'
              AND (OBJECT_TYPE <> 'SYNONYM' OR OWNER NOT IN ({ignoredOwners}))
            """;

        await using var command = connection.CreateTextCommand(sql);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task<IReadOnlyList<CompileStatement>> GetCompileStatementsAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        var statements = new List<CompileStatement>();

        const string codeSql = """
            SELECT OWNER, OBJECT_NAME, OBJECT_TYPE,
                   CASE OBJECT_TYPE
                       WHEN 'TYPE' THEN 1 WHEN 'TYPE BODY' THEN 2
                       WHEN 'PACKAGE' THEN 3 WHEN 'PACKAGE BODY' THEN 4
                       ELSE 5
                   END AS COMPILE_ORDER
            FROM ALL_OBJECTS
            WHERE OBJECT_TYPE IN ('TYPE', 'TYPE BODY', 'PACKAGE', 'PACKAGE BODY', 'PROCEDURE', 'FUNCTION', 'TRIGGER', 'VIEW')
              AND STATUS <> 'VALID'
              AND OWNER <> 'SYS'
            ORDER BY CASE WHEN OBJECT_TYPE IN ('PROCEDURE', 'FUNCTION', 'TRIGGER', 'VIEW') THEN 1 ELSE 0 END,
                     OBJECT_NAME, COMPILE_ORDER
            """;

        await using (var command = connection.CreateTextCommand(codeSql))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var owner = reader.GetString(0);
                var name = reader.GetString(1);
                var type = reader.GetString(2);
                var target = OracleExtensions.Quote(owner, name);

                var sql = type switch
                {
                    "PACKAGE BODY" => $"ALTER PACKAGE {target} COMPILE BODY",
                    "TYPE BODY" => $"ALTER TYPE {target} COMPILE BODY",
                    _ => $"ALTER {type} {target} COMPILE"
                };

                statements.Add(new CompileStatement($"{type}: {owner}.{name}", sql));
            }
        }

        const string synonymSql = """
            SELECT DISTINCT O.OWNER, O.OBJECT_NAME, S.TABLE_OWNER, S.TABLE_NAME
            FROM ALL_OBJECTS O
                INNER JOIN ALL_SYNONYMS S
                ON O.OWNER = S.OWNER
                AND O.OBJECT_NAME = S.SYNONYM_NAME
            WHERE O.OBJECT_TYPE = 'SYNONYM'
              AND O.STATUS <> 'VALID'
            ORDER BY O.OBJECT_NAME
            """;

        await using (var command = connection.CreateTextCommand(synonymSql))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var owner = reader.GetString(0);
                var name = reader.GetString(1);
                var target = OracleExtensions.Quote(reader.GetStringOrEmpty(2), reader.GetStringOrEmpty(3));

                statements.Add(owner == "PUBLIC"
                    ? new CompileStatement($"Sinônimo Público: {name}", $"CREATE OR REPLACE PUBLIC SYNONYM {OracleExtensions.Quote(name)} FOR {target}")
                    : new CompileStatement($"Sinônimo: {owner}.{name}", $"ALTER SYNONYM {OracleExtensions.Quote(owner, name)} COMPILE"));
            }
        }

        return statements;
    }
}
