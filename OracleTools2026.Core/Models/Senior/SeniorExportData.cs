namespace OracleTools2026.Models.Senior;

// Os erros por item (regra que não decodificou, DDL não gerada) ficam nos próprios itens
// (SeniorPort.RuleError, SeniorTableDdl.Error) e não interrompem a exportação.
public sealed record SeniorExportData(
    string Owner,
    IReadOnlyList<SeniorService> Services,
    IReadOnlyList<SeniorTable> Tables,
    IReadOnlyList<SeniorField> StandardTableFields,
    IReadOnlyList<SeniorList> Lists,
    IReadOnlyList<SeniorView> Views,
    IReadOnlyList<SeniorRuleLink> RuleLinks,
    IReadOnlyList<SeniorTableDdl> TableDdls)
{
    // Um item por arquivo principal da exportação, na ordem em que são gravados.
    public IReadOnlyList<SeniorItemSummary> Items(SeniorExportOptions options)
    {
        var items = new List<SeniorItemSummary>();
        if (options.IncludeWebServices)
        {
            items.AddRange(Services.Select(s => new SeniorItemSummary(
                new SeniorItemKey(SeniorItemKind.WebService, s.Name), $"{s.Ports.Count} porta(s)", s.LastChange)));
        }

        if (options.IncludeCbds || options.IncludeTableDdl)
        {
            var detail = options.IncludeCbds ? null : "só DDL";
            items.AddRange(Tables.Select(t => new SeniorItemSummary(
                new SeniorItemKey(SeniorItemKind.Table, t.Name),
                detail ?? $"{t.Fields.Count} campo(s), {t.Indexes.Count} índice(s)",
                t.LastChange)));

            // Sem o CBDS, a exportação lê só os nomes das tabelas para gerar o DDL.
            var tableNames = Tables.Select(t => t.Name).ToHashSet(StringComparer.Ordinal);
            items.AddRange(TableDdls
                .Where(d => !tableNames.Contains(d.TableName))
                .Select(d => new SeniorItemSummary(new SeniorItemKey(SeniorItemKind.Table, d.TableName), "só DDL", null)));
        }

        if (options.IncludeCbds)
        {
            items.AddRange(StandardTableFields
                .GroupBy(f => f.TableName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new SeniorItemSummary(
                    new SeniorItemKey(SeniorItemKind.StandardTableFields, g.Key),
                    $"{g.Count()} campo(s) USU_*",
                    g.Max(f => f.LastChange))));
            items.AddRange(Lists.Select(l => new SeniorItemSummary(
                new SeniorItemKey(SeniorItemKind.List, l.Name), $"{l.Items.Count} item(ns)", l.LastChange)));
            items.AddRange(Views.Select(v => new SeniorItemSummary(
                new SeniorItemKey(SeniorItemKind.View, v.Name), "", v.LastChange)));
        }

        if (options.IncludeRuleLinks)
        {
            items.AddRange(RuleLinks.Select(r => new SeniorItemSummary(r.Key, $"{r.Links.Count} vínculo(s)", r.LastChange)));
        }

        return items;
    }

    public SeniorExportData Filter(IReadOnlySet<SeniorItemKey> selection)
    {
        // Os campos são agrupados por tabela sem diferenciar maiúsculas (igual ao SeniorExportWriter).
        var standardTables = selection
            .Where(k => k.Kind == SeniorItemKind.StandardTableFields)
            .Select(k => k.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return this with
        {
            Services = Services.Where(s => selection.Contains(new SeniorItemKey(SeniorItemKind.WebService, s.Name))).ToList(),
            Tables = Tables.Where(t => selection.Contains(new SeniorItemKey(SeniorItemKind.Table, t.Name))).ToList(),
            StandardTableFields = StandardTableFields.Where(f => standardTables.Contains(f.TableName)).ToList(),
            Lists = Lists.Where(l => selection.Contains(new SeniorItemKey(SeniorItemKind.List, l.Name))).ToList(),
            Views = Views.Where(v => selection.Contains(new SeniorItemKey(SeniorItemKind.View, v.Name))).ToList(),
            RuleLinks = RuleLinks.Where(r => selection.Contains(r.Key)).ToList(),
            TableDdls = TableDdls.Where(d => selection.Contains(new SeniorItemKey(SeniorItemKind.Table, d.TableName))).ToList()
        };
    }
}
