namespace OracleTools2026.Models.Senior;

public sealed record SeniorIndex(string TableName, string Name, IReadOnlyList<SeniorAttribute> Attributes, IReadOnlyList<string> Fields);
