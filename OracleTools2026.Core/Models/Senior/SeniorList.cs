namespace OracleTools2026.Models.Senior;

public sealed record SeniorList(string Name, IReadOnlyList<SeniorAttribute> Attributes, IReadOnlyList<SeniorRecord> Items, DateTime? LastChange);
