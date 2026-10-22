namespace OracleTools2026.Models;

public sealed record DatabaseUser(
    string Name,
    DateTime? CreatedAt,
    string Profile,
    string Status,
    DateTime? LockDate,
    DateTime? ExpiryDate,
    DateTime? LastLogin);
