namespace OracleTools2026.Models;

public sealed record DirectoryUser(
    string Login,
    string DisplayName,
    string Description,
    string Email,
    string Department,
    string Title,
    bool Disabled,
    bool LockedOut,
    bool PasswordNeverExpires,
    bool MustChangePassword,
    DateTime? LastLogon,
    DateTime? PasswordLastSet,
    DateTime? AccountExpires,
    IReadOnlyList<string> Groups);
