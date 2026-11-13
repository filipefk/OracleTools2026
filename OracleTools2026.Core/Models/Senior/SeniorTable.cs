namespace OracleTools2026.Models.Senior;

public sealed record SeniorTable(
    string Name,
    IReadOnlyList<SeniorAttribute> Attributes,
    IReadOnlyList<SeniorField> Fields,
    IReadOnlyList<SeniorIndex> Indexes,
    DateTime? LastChange);
