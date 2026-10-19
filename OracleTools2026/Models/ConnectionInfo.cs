using Oracle.ManagedDataAccess.Client;

namespace OracleTools2026.Models;

public sealed record ConnectionInfo(string User, string Password, string DataSource)
{
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(User) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(DataSource);

    public string BuildConnectionString() => new OracleConnectionStringBuilder
    {
        UserID = User,
        Password = Password,
        DataSource = DataSource
    }.ConnectionString;
}
