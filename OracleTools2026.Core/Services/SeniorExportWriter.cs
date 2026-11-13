using OracleTools2026.Formatting;
using OracleTools2026.Models;
using OracleTools2026.Models.Senior;
using System.Text;

namespace OracleTools2026.Services;

// Grava a árvore senior/ do repositório de artefatos (UTF-8 sem BOM, fim de linha LF) e encontra os
// arquivos órfãos: os que estão nas pastas dos tipos exportados mas não saíram desta exportação.
public sealed class SeniorExportWriter
{
    private const string CbdsVolatileComment = "Fora do arquivo por serem voláteis: LASUPD e CODUSU";

    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    // Grava item a item (os arquivos de cada serviço, tabela, lista...) e informa o resultado de cada um.
    // Os erros do item (regra que não decodificou, DDL não gerada, nome repetido) não interrompem a gravação.
    public async Task<SeniorWriteResult> WriteAsync(
        SeniorExportOptions options, SeniorExportData data, SeniorDependencyScan? dependencies,
        IProgress<ProgressInfo>? progress = null, IProgress<SeniorItemProgress>? itemProgress = null,
        CancellationToken cancellationToken = default)
    {
        var root = options.RootFolder;
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<ItemFiles>();
        var byKey = new Dictionary<SeniorItemKey, ItemFiles>();
        var kept = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ItemFiles Item(SeniorItemKey? key)
        {
            if (key is not null && byKey.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var item = new ItemFiles(key);
            items.Add(item);
            if (key is not null)
            {
                byKey[key] = item;
            }

            return item;
        }

        void Add(SeniorItemKey? key, string content, params string[] parts)
        {
            var path = Path.Combine([root, .. parts.Select(FileNames.ToSafeFileName)]);
            var item = Item(key);
            if (paths.Add(path))
            {
                item.Files.Add((path, content));
            }
            else
            {
                item.Errors.Add($"Arquivo gerado mais de uma vez (nomes repetidos no banco): {Path.GetRelativePath(root, path)}");
            }
        }

        if (options.IncludeWebServices)
        {
            foreach (var service in data.Services)
            {
                var key = new SeniorItemKey(SeniorItemKind.WebService, service.Name);
                Add(key, BuildService(data.Owner, service), "webservices", service.Name, "servico.yaml");
                foreach (var port in service.Ports)
                {
                    Add(key, BuildPort(data.Owner, port), "webservices", service.Name, port.Name, "porta.yaml");
                    if (port.ParametersXml is not null)
                    {
                        Add(key, EndWithNewLine(port.ParametersXml), "webservices", service.Name, port.Name, "parametros.xml");
                    }

                    if (port.RuleSource is not null)
                    {
                        Add(key, EndWithNewLine(port.RuleSource), "webservices", service.Name, port.Name, "regra.lsp");
                    }
                    else if (port.RuleError is not null)
                    {
                        // Não apaga a versão anterior do fonte só porque a decodificação falhou agora.
                        Item(key).Errors.Add($"Regra da porta {port.QualifiedName}: {port.RuleError}");
                        kept.Add(Path.Combine(root, "webservices", FileNames.ToSafeFileName(service.Name), FileNames.ToSafeFileName(port.Name), "regra.lsp"));
                    }
                }
            }
        }

        if (options.IncludeCbds)
        {
            foreach (var table in data.Tables)
            {
                Add(new SeniorItemKey(SeniorItemKind.Table, table.Name), BuildTable(data.Owner, table), "cbds", "tabelas", table.Name + ".yaml");
            }
        }

        if (options.IncludeTableDdl)
        {
            foreach (var ddl in data.TableDdls)
            {
                var key = new SeniorItemKey(SeniorItemKind.Table, ddl.TableName);
                if (ddl.Ddl is not null)
                {
                    Add(key, BuildTableDdl(data.Owner, ddl.Ddl), "cbds", "tabelas", ddl.TableName + ".oracle.sql");
                }
                else
                {
                    Item(key).Errors.Add($"DDL da tabela {ddl.TableName}: {ddl.Error}");
                    kept.Add(Path.Combine(root, "cbds", "tabelas", FileNames.ToSafeFileName(ddl.TableName + ".oracle.sql")));
                }
            }
        }

        if (options.IncludeCbds)
        {
            foreach (var group in data.StandardTableFields.GroupBy(f => f.TableName, StringComparer.OrdinalIgnoreCase))
            {
                Add(new SeniorItemKey(SeniorItemKind.StandardTableFields, group.Key),
                    BuildStandardTableFields(data.Owner, group.Key, group.ToList()), "cbds", "campos", group.Key + ".yaml");
            }

            foreach (var list in data.Lists)
            {
                Add(new SeniorItemKey(SeniorItemKind.List, list.Name), BuildList(data.Owner, list), "cbds", "listas", list.Name + ".yaml");
            }

            foreach (var view in data.Views)
            {
                Add(new SeniorItemKey(SeniorItemKind.View, view.Name), BuildView(data.Owner, view), "cbds", "visoes", view.Name + ".yaml");
            }
        }

        if (options.IncludeRuleLinks)
        {
            foreach (var rule in data.RuleLinks)
            {
                Add(rule.Key, BuildRuleLinks(data.Owner, rule), "regras", rule.Module, rule.Identifier, "vinculos.yaml");
            }
        }

        // O dependencias.yaml não é de um item: é gravado por último, sempre com todos os serviços lidos.
        if (options.IncludeWebServices)
        {
            Add(null, BuildDependencies(dependencies), "dependencias.yaml");
        }

        var written = 0;
        var unchanged = 0;
        var errors = new List<string>();
        var outcomes = new List<SeniorItemProgress>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            progress?.Report(new ProgressInfo(i + 1, items.Count,
                item.Files.Count > 0 ? $"Gravando {Path.GetRelativePath(root, item.Files[0].Path)}" : "Gravando"));

            var itemWritten = 0;
            foreach (var (path, content) in item.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await WriteFileAsync(path, content, cancellationToken))
                {
                    itemWritten++;
                }
            }

            written += itemWritten;
            unchanged += item.Files.Count - itemWritten;
            errors.AddRange(item.Errors);

            if (item.Key is not null)
            {
                var outcome = new SeniorItemProgress(
                    item.Key,
                    item.Errors.Count > 0 ? SeniorItemOutcome.Error : itemWritten > 0 ? SeniorItemOutcome.Written : SeniorItemOutcome.Unchanged,
                    item.Errors.Count > 0 ? string.Join(" | ", item.Errors) : null);
                outcomes.Add(outcome);
                itemProgress?.Report(outcome);
            }
        }

        kept.UnionWith(paths);
        return new SeniorWriteResult(kept, written, unchanged, errors, outcomes);
    }

    // Arquivos iguais não são regravados, para não mexer na data de alteração.
    private static async Task<bool> WriteFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        var bytes = FileEncoding.GetBytes(content);
        if (File.Exists(path) && (await File.ReadAllBytesAsync(path, cancellationToken)).AsSpan().SequenceEqual(bytes))
        {
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        return true;
    }

    public IReadOnlyList<string> FindOrphans(SeniorExportOptions options, IReadOnlyCollection<string> exportedFiles)
    {
        var root = options.RootFolder;
        if (!Directory.Exists(root))
        {
            return [];
        }

        var exported = new HashSet<string>(exportedFiles, StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !exported.Contains(path) && IsInExportedScope(options, Path.GetRelativePath(root, path)))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public int DeleteOrphans(SeniorExportOptions options, IReadOnlyList<string> orphans)
    {
        var root = Path.GetFullPath(options.RootFolder);
        var deleted = 0;
        foreach (var path in orphans)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                deleted++;
            }
        }

        // Remove as pastas que ficaram vazias (serviços, portas e regras removidos), sem tocar na raiz.
        foreach (var folder in orphans
                     .Select(path => Path.GetDirectoryName(path))
                     .OfType<string>()
                     .SelectMany(folder => AncestorsUpToRoot(folder))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderByDescending(f => f.Length))
        {
            if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
            {
                Directory.Delete(folder);
            }
        }

        return deleted;

        IEnumerable<string> AncestorsUpToRoot(string folder)
        {
            for (var current = Path.GetFullPath(folder);
                 current.Length > root.Length && current.StartsWith(root, StringComparison.OrdinalIgnoreCase);
                 current = Path.GetDirectoryName(current)!)
            {
                yield return current;
            }
        }
    }

    // Só são órfãos os arquivos das pastas dos tipos marcados; pastas e arquivos ocultos (".git"...) e o
    // que estiver fora das pastas conhecidas (ex.: um README na raiz) nunca são apagados.
    private static bool IsInExportedScope(SeniorExportOptions options, string relativePath)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Any(p => p.StartsWith('.')))
        {
            return false;
        }

        var first = parts[0].ToLowerInvariant();
        return first switch
        {
            "webservices" => options.IncludeWebServices,
            "dependencias.yaml" => options.IncludeWebServices && parts.Length == 1,
            "regras" => options.IncludeRuleLinks,
            "cbds" when relativePath.EndsWith(".oracle.sql", StringComparison.OrdinalIgnoreCase) => options.IncludeTableDdl,
            "cbds" => options.IncludeCbds,
            _ => false
        };
    }

    private static string BuildService(string owner, SeniorService service)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R960SRV (schema {owner})")
            .Value("servico", service.Name);
        WriteAttributes(yaml, service.Attributes);

        if (service.Ports.Count == 0)
        {
            yaml.Value("portas", Array.Empty<string>());
        }
        else
        {
            yaml.Section("portas");
            foreach (var port in service.Ports)
            {
                yaml.Item(port.Name);
            }
        }

        return yaml.ToString();
    }

    private static string BuildPort(string owner, SeniorPort port)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R960PRT (schema {owner})")
            .Comment("Fora do arquivo por ser volátil: PRMDTM (data) e o <changeHistory> do PRMDEF")
            .Value("servico", port.ServiceName)
            .Value("porta", port.Name);
        WriteAttributes(yaml, port.Attributes);

        if (port.ParametersXml is null && port.RuleSource is null && port.RuleError is null)
        {
            return yaml.EmptyMap("arquivos").ToString();
        }

        yaml.Section("arquivos");
        if (port.ParametersXml is not null)
        {
            yaml.Value("parametros", "parametros.xml", "PRMDEF", indent: 2);
        }

        if (port.RuleSource is not null || port.RuleError is not null)
        {
            yaml.Value("regra", "regra.lsp", "R960RUL.RULDAT decodificado", indent: 2);
        }

        return yaml.ToString();
    }

    private static string BuildDependencies(SeniorDependencyScan? scan)
    {
        var yaml = new YamlText()
            .Comment("Montado a partir das linhas \"Definir interno.<serviço>\" e dos nomes USU_T* no fonte das regras.")
            .Comment("Ordem de implantação: CBDS, depois webservices, depois regras.");

        var dependencies = scan?.Dependencies ?? [];
        if (dependencies.Count == 0)
        {
            return yaml.EmptyMap("webservices").ToString();
        }

        yaml.Section("webservices");
        foreach (var dependency in dependencies)
        {
            yaml.Section(dependency.Source, indent: 2);
            if (dependency.Calls.Count == 0)
            {
                yaml.Value("chama", Array.Empty<string>(), indent: 4);
            }
            else
            {
                yaml.Section("chama", indent: 4);
                foreach (var call in dependency.Calls)
                {
                    yaml.Item(call.Name, call.IsStandard ? "padrão Senior" : null, indent: 6);
                }
            }

            yaml.Value("usa_tabelas", dependency.Tables, indent: 4);
        }

        return yaml.ToString();
    }

    private static string BuildTable(string owner, SeniorTable table)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R998TBL, R998FLD, R998IND, R998FND (schema {owner})")
            .Comment(CbdsVolatileComment)
            .Value("tabela", table.Name);
        WriteAttributes(yaml, table.Attributes);

        yaml.BlankLine();
        WriteFields(yaml, table.Fields);

        yaml.BlankLine();
        if (table.Indexes.Count == 0)
        {
            yaml.Value("indices", Array.Empty<string>());
        }
        else
        {
            yaml.Section("indices");
            foreach (var index in table.Indexes)
            {
                yaml.MapItem("nome", index.Name);
                WriteAttributes(yaml, index.Attributes, indent: 4);
                yaml.Value("campos", index.Fields, indent: 4);
            }
        }

        return yaml.ToString();
    }

    private static string BuildStandardTableFields(string owner, string tableName, IReadOnlyList<SeniorField> fields)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R998FLD, campos USU_* da tabela padrão {tableName} (schema {owner})")
            .Comment(CbdsVolatileComment)
            .Value("tabela", tableName);
        WriteFields(yaml, fields);
        return yaml.ToString();
    }

    private static void WriteFields(YamlText yaml, IReadOnlyList<SeniorField> fields)
    {
        if (fields.Count == 0)
        {
            yaml.Value("campos", Array.Empty<string>());
            return;
        }

        yaml.Section("campos");
        foreach (var field in fields)
        {
            yaml.InlineMapItem(field.Attributes.Select(a => (a.Key, a.Value)).Prepend(("nome", (object?)field.Name)));
        }
    }

    private static string BuildList(string owner, SeniorList list)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R998LST, R998LSF (schema {owner})")
            .Comment(CbdsVolatileComment)
            .Value("lista", list.Name);
        WriteAttributes(yaml, list.Attributes);
        WriteRecords(yaml, "itens", list.Items);
        return yaml.ToString();
    }

    private static string BuildView(string owner, SeniorView view)
    {
        var yaml = new YamlText()
            .Comment($"Origem: R998VIW (schema {owner}), SQL concatenado na ordem de VIWSEQ")
            .Comment(CbdsVolatileComment)
            .Value("visao", view.Name);
        WriteAttributes(yaml, view.Attributes);
        return yaml.Block("sql", view.Sql).ToString();
    }

    private static string BuildRuleLinks(string owner, SeniorRuleLink rule)
    {
        var yaml = new YamlText()
            .Comment($"Origem: E098REG (schema {owner})")
            .Comment("Fora do arquivo por serem voláteis: USUGER, DATGER, HORGER")
            .Value("modulo", rule.Module)
            .Value("identificador", rule.Identifier);
        WriteAttributes(yaml, rule.Attributes);
        WriteRecords(yaml, "vinculos", rule.Links);
        return yaml.ToString();
    }

    private static string BuildTableDdl(string owner, string ddl) =>
        $"""
        -- Referência apenas: DDL real no Oracle (DBMS_METADATA.GET_DDL), schema {owner}.
        -- Não aplicar direto: a implantação é pelo CBDS.

        {ddl}

        """.ReplaceLineEndings("\n");

    private static void WriteRecords(YamlText yaml, string key, IReadOnlyList<SeniorRecord> records)
    {
        if (records.Count == 0)
        {
            yaml.Value(key, Array.Empty<string>());
            return;
        }

        yaml.Section(key);
        foreach (var record in records)
        {
            yaml.InlineMapItem(record.Attributes.Select(a => (a.Key, a.Value)));
        }
    }

    // Quando o nome no YAML não é o da coluna, a coluna de origem vai como comentário (como nas amostras).
    private static void WriteAttributes(YamlText yaml, IReadOnlyList<SeniorAttribute> attributes, int indent = 0)
    {
        foreach (var attribute in attributes)
        {
            var comment = attribute.Key.Equals(attribute.Column, StringComparison.OrdinalIgnoreCase) ? null : attribute.Column;
            yaml.Value(attribute.Key, attribute.Value, comment, indent);
        }
    }

    private static string EndWithNewLine(string text) =>
        text.Length == 0 || text[^1] == '\n' ? text : text + "\n";

    private sealed class ItemFiles(SeniorItemKey? key)
    {
        public SeniorItemKey? Key { get; } = key;

        public List<(string Path, string Content)> Files { get; } = [];

        public List<string> Errors { get; } = [];
    }
}
