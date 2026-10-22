namespace OracleTools2026.Models;

public sealed record UserDetails(
    string Name,
    string Status,
    string DefaultTablespace,
    string TemporaryTablespace,
    string Profile,
    DateTime? CreatedAt,
    DateTime? LockDate,
    DateTime? ExpiryDate,
    DateTime? LastLogin);
