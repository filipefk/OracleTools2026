namespace OracleTools2026.Models;

public sealed record DatabaseLink(DateTime? CreatedAt, string Owner, string Name, string UserName, string Host);
