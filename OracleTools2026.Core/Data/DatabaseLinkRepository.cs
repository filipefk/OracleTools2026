using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Text.RegularExpressions;

namespace OracleTools2026.Data;

public sealed partial class DatabaseLinkRepository(IOracleSession session)
{
    private const int TestTimeoutSeconds = 60;

    public async Task<IReadOnlyList<DatabaseLink>> GetLinksAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CREATED,
                   OWNER,
                   DB_LINK,
                   USERNAME,
                   HOST
            FROM ALL_DB_LINKS
            ORDER BY OWNER, DB_LINK
            """;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var links = new List<DatabaseLink>();
        while (await reader.ReadAsync(cancellationToken))
        {
            links.Add(new DatabaseLink(
                reader.GetDateTimeOrNull(0),
                reader.GetStringOrEmpty(1),
                reader.GetStringOrEmpty(2),
                reader.GetStringOrEmpty(3),
                reader.GetStringOrEmpty(4)));
        }

        return links;
    }

    public async Task<IReadOnlyList<DatabaseLinkTestResult>> TestAsync(
        IReadOnlyList<DatabaseLink> links,
        IProgress<ProgressInfo>? progress = null,
        IProgress<DatabaseLinkTestResult>? testedLink = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        var results = new List<DatabaseLinkTestResult>(links.Count);
        for (var i = 0; i < links.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var link = links[i];
            progress?.Report(new ProgressInfo(i + 1, links.Count, link.Name));

            var result = new DatabaseLinkTestResult(link, await TestLinkAsync(connection, link.Name, cancellationToken));
            results.Add(result);
            testedLink?.Report(result);
        }

        return results;
    }

    private static async Task<string?> TestLinkAsync(OracleConnection connection, string linkName, CancellationToken cancellationToken)
    {
        // O nome do DBLINK não pode ser bind variable; só nomes com caracteres válidos sem aspas são concatenados.
        if (!LinkNamePattern().IsMatch(linkName))
        {
            return "Nome de DBLINK não suportado para teste.";
        }

        try
        {
            await using var command = connection.CreateTextCommand($"SELECT DUMMY FROM DUAL@{linkName}");
            command.CommandTimeout = TestTimeoutSeconds;
            await using var cancelRegistration = cancellationToken.Register(command.Cancel);
            await command.ExecuteScalarAsync(cancellationToken);
            return null;
        }
        catch (OracleException) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        catch (OracleException ex)
        {
            return ex.Message;
        }
        finally
        {
            await CloseLinkAsync(connection, linkName);
        }
    }

    // Fecha o link depois de cada teste para não estourar o limite de links abertos por sessão
    // (ORA-02020), que acontecia no original ao testar todos. O ROLLBACK encerra a transação
    // distribuída aberta pela consulta; sem ele o link não pode ser fechado (ORA-02080).
    private static async Task CloseLinkAsync(OracleConnection connection, string linkName)
    {
        foreach (var sql in new[] { "ROLLBACK", $"ALTER SESSION CLOSE DATABASE LINK {linkName}" })
        {
            try
            {
                await using var command = connection.CreateTextCommand(sql);
                await command.ExecuteNonQueryAsync();
            }
            catch (OracleException)
            {
                // O link pode já estar fechado ou nem ter sido aberto.
            }
        }
    }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_$#.@]*$")]
    private static partial Regex LinkNamePattern();
}
