namespace OracleTools2026.Models.Senior;

public sealed record SeniorField(string TableName, string Name, IReadOnlyList<SeniorAttribute> Attributes, DateTime? LastChange);
