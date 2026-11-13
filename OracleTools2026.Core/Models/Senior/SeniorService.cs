namespace OracleTools2026.Models.Senior;

public sealed record SeniorService(string Name, IReadOnlyList<SeniorAttribute> Attributes, IReadOnlyList<SeniorPort> Ports, DateTime? LastChange);
