namespace OracleTools2026.Models;

public sealed record SavedDirectoryConnection(string Alias, DirectoryConnection Connection)
{
    public bool HasPassword => !string.IsNullOrWhiteSpace(Connection.Password);
}
