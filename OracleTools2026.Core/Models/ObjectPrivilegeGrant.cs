namespace OracleTools2026.Models;

// Column é preenchida apenas nos privilégios concedidos em colunas específicas (DBA_COL_PRIVS).
public sealed record ObjectPrivilegeGrant(
    string Owner,
    string ObjectName,
    string ObjectType,
    string Privilege,
    bool Grantable,
    string? Column = null)
{
    public string QualifiedName => $"{Owner}.{ObjectName}";

    public bool IsDirectory => ObjectType == "DIRECTORY";
}
