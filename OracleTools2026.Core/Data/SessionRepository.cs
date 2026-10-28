using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Globalization;

namespace OracleTools2026.Data;

public sealed class SessionRepository(IOracleSession session)
{
    public async Task<IReadOnlyList<DatabaseSession>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT INST_ID,
                   SID,
                   SERIAL#,
                   USERNAME,
                   OSUSER,
                   MACHINE,
                   STATUS,
                   PROGRAM,
                   TYPE,
                   MODULE,
                   LOGON_TIME,
                   STATE
            FROM GV$SESSION
            ORDER BY INST_ID, SID
            """;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var sessions = new List<DatabaseSession>();
        while (await reader.ReadAsync(cancellationToken))
        {
            sessions.Add(new DatabaseSession(
                (int)reader.GetInt64OrZero(0),
                reader.GetInt64OrZero(1),
                reader.GetInt64OrZero(2),
                reader.GetStringOrEmpty(3),
                reader.GetStringOrEmpty(4),
                reader.GetStringOrEmpty(5),
                reader.GetStringOrEmpty(6),
                reader.GetStringOrEmpty(7),
                reader.GetStringOrEmpty(8),
                reader.GetStringOrEmpty(9),
                reader.GetDateTimeOrNull(10),
                reader.GetStringOrEmpty(11)));
        }

        return sessions;
    }

    public async Task<IReadOnlyList<SessionKillResult>> KillAsync(
        IReadOnlyList<DatabaseSession> sessions,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        var currentInstance = await GetCurrentInstanceAsync(connection, cancellationToken);

        var results = new List<SessionKillResult>(sessions.Count);
        for (var i = 0; i < sessions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = sessions[i];
            progress?.Report(new ProgressInfo(i + 1, sessions.Count, $"Sessão {target.Sid},{target.SerialNumber}"));

            try
            {
                await using var command = connection.CreateTextCommand(BuildKillCommand(target, currentInstance));
                await command.ExecuteNonQueryAsync(cancellationToken);
                results.Add(new SessionKillResult(target, null));
            }
            catch (OracleException ex)
            {
                results.Add(new SessionKillResult(target, ex.Message));
            }
        }

        return results;
    }

    // DDL não aceita bind variables; o comando é montado apenas com os números lidos do próprio GV$SESSION.
    // Na instância da conexão usa o mesmo DISCONNECT do original; em outra instância do RAC usa
    // KILL SESSION com o "@inst_id", que o DISCONNECT não aceita.
    private static string BuildKillCommand(DatabaseSession target, int currentInstance) =>
        target.InstanceId == currentInstance || currentInstance == 0
            ? string.Create(CultureInfo.InvariantCulture, $"ALTER SYSTEM DISCONNECT SESSION '{target.Sid},{target.SerialNumber}' IMMEDIATE")
            : string.Create(CultureInfo.InvariantCulture, $"ALTER SYSTEM KILL SESSION '{target.Sid},{target.SerialNumber},@{target.InstanceId}' IMMEDIATE");

    private static async Task<int> GetCurrentInstanceAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateTextCommand("SELECT SYS_CONTEXT('USERENV', 'INSTANCE') FROM DUAL");
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var instance) ? instance : 0;
    }
}
