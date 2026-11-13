namespace OracleTools2026.Models.Senior;

// Selection: itens marcados na lista; null exporta tudo o que for lido.
public sealed record SeniorExportOptions(
    string Owner,
    string Folder,
    bool IncludeWebServices,
    bool IncludeCbds,
    bool IncludeRuleLinks,
    bool IncludeTableDdl,
    IReadOnlySet<SeniorItemKey>? Selection = null)
{
    public bool HasAnyType => IncludeWebServices || IncludeCbds || IncludeRuleLinks || IncludeTableDdl;

    public bool IsPartial => Selection is not null;

    // A exportação sempre grava na subpasta senior, que é a raiz do repositório de artefatos.
    public string RootFolder => Path.Combine(Folder, "senior");

    public bool IsSelected(SeniorItemKey key) => Selection is null || Selection.Contains(key);
}
