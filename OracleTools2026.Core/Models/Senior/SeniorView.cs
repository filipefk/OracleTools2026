namespace OracleTools2026.Models.Senior;

public sealed record SeniorView(string Name, IReadOnlyList<SeniorAttribute> Attributes, string Sql, DateTime? LastChange);
