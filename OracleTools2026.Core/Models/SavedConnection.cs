namespace OracleTools2026.Models;

public sealed record SavedConnection(string Alias, string User, string Password, string DataSource)
{
    public bool HasPassword => !string.IsNullOrWhiteSpace(Password);
}
