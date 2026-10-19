using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;

namespace OracleTools2026.Data;

public interface IOracleSession
{
    ConnectionInfo? Current { get; }
    bool IsConnected { get; }

    event EventHandler? ConnectionChanged;

    Task TestAsync(ConnectionInfo connection, CancellationToken cancellationToken = default);
    Task ConnectAsync(ConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<OracleConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
    Task<bool> HasAuditTablesAsync(OracleConnection connection, CancellationToken cancellationToken = default);
}
