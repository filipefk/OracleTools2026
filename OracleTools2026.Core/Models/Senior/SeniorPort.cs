namespace OracleTools2026.Models.Senior;

public sealed record SeniorPort(
    string ServiceName,
    string Name,
    IReadOnlyList<SeniorAttribute> Attributes,
    string? ParametersXml,
    string? RuleSource,
    string? RuleError)
{
    public string QualifiedName => $"{ServiceName}/{Name}";
}
