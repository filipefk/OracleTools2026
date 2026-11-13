namespace OracleTools2026.Models.Senior;

// Identifica um item da exportação. Name: nome do serviço, tabela, lista ou visão; nome da tabela padrão
// nos campos de usuário; MODSIS/IDEREG nos vínculos de regras.
public sealed record SeniorItemKey(SeniorItemKind Kind, string Name)
{
    public static SeniorItemKey RuleLink(string module, string identifier) => new(SeniorItemKind.RuleLink, $"{module}/{identifier}");
}
