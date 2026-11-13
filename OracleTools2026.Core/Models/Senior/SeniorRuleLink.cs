namespace OracleTools2026.Models.Senior;

// Vínculos de uma regra (E098REG) agrupados por módulo e identificador.
public sealed record SeniorRuleLink(
    string Module,
    string Identifier,
    IReadOnlyList<SeniorAttribute> Attributes,
    IReadOnlyList<SeniorRecord> Links,
    DateTime? LastChange)
{
    public SeniorItemKey Key => SeniorItemKey.RuleLink(Module, Identifier);
}
