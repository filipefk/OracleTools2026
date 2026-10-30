using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using OracleTools2026.Settings;

namespace OracleTools2026.Data;

public sealed class OracleSession(IAppSettings settings) : IOracleSession
{
    public ConnectionInfo? Current { get; private set; }

    public bool IsConnected => Current?.IsComplete == true;

    public event EventHandler? ConnectionChanged;

    public async Task TestAsync(ConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        await using var oracleConnection = CreateConnection(connection);
        await oracleConnection.OpenAsync(cancellationToken);
    }

    public async Task ConnectAsync(ConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        await TestAsync(connection, cancellationToken);

        Current = connection;
        settings.LastLoginUser = connection.StoredUser;
        settings.LastLoginDataSource = connection.DataSource;
        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<OracleConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (Current is not { IsComplete: true } current)
        {
            throw new InvalidOperationException("Não há conexão com o banco de dados.");
        }

        var connection = CreateConnection(current);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    // OracleConfiguration.TnsAdmin é global e não pode ser alterado depois que a primeira conexão
    // é aberta (ORA-50099); por isso o TNS_ADMIN é definido em cada conexão.
    private OracleConnection CreateConnection(ConnectionInfo connection)
    {
        var oracleConnection = new OracleConnection(connection.BuildConnectionString());
        if (TnsNamesParser.GetTnsAdminFolder(settings.TnsNamesPath) is { } tnsAdmin)
        {
            oracleConnection.TnsAdmin = tnsAdmin;
        }

        return oracleConnection;
    }
}
