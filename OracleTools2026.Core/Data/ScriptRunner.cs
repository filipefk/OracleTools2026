using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;

namespace OracleTools2026.Data;

public sealed class ScriptRunner(IOracleSession session)
{
    public async Task<QueryResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(NormalizeStatement(sql));
        await using var cancelRegistration = cancellationToken.Register(command.Cancel);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
        var rows = new List<object?[]>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new object?[reader.FieldCount];
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = ReadValue(reader, i);
            }

            rows.Add(row);
        }

        return new QueryResult(columns, rows);
    }

    // Scripts salvos para o SQL*Plus costumam terminar com ";" ou "/", que o driver não aceita em comandos SQL.
    internal static string NormalizeStatement(string sql)
    {
        var lines = sql.Trim().ReplaceLineEndings("\n").Split('\n').ToList();
        while (lines.Count > 0 && lines[^1].Trim() is "" or "/")
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var text = string.Join("\n", lines).Trim();
        var isPlSqlBlock = text.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase) ||
                           text.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase);

        return isPlSqlBlock ? text : text.TrimEnd(';').TrimEnd();
    }

    private static object? ReadValue(OracleDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        try
        {
            return reader.GetValue(ordinal);
        }
        catch (Exception ex) when (ex is InvalidCastException or OverflowException)
        {
            // NUMBER com mais dígitos do que o decimal do .NET suporta, por exemplo.
            return reader.GetProviderSpecificValue(ordinal)?.ToString();
        }
    }
}
