using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using OracleTools2026.Models.Senior;
using System.Text.RegularExpressions;

namespace OracleTools2026.Data.Senior;

// Exceção documentada no CLAUDE.md: única classe que consulta tabelas de aplicação (as do Senior), só com
// SELECT e só em R960SRV, R960PRT, R960RUL, R998* e E098REG. A R960PRV (credenciais) nunca é lida.
// O schema não pode ser bind variable no FROM: só é aceito se estiver na lista de GetOwnersAsync e entra
// no SQL com OracleExtensions.Quote.
public sealed partial class SeniorRepository(IOracleSession session, SeniorRuleDecoder decoder)
{
    private const string StandardServicePrefix = "com.senior.";

    private static readonly string[] ViewTextColumns = ["VIWSQL", "VIWTXT", "VIWDEF", "SQLVIW", "TXTVIW", "SQLTXT", "SQLCMD"];

    public async Task<IReadOnlyList<string>> GetOwnersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetOwnersAsync(connection, cancellationToken);
    }

    public Task<SeniorExportData> ExportAsync(
        SeniorExportOptions options, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default) =>
        ReadAsync(options, listing: false, progress, cancellationToken);

    // Itens que a exportação geraria, para a lista da tela: não lê nem decodifica as regras (R960RUL) e não
    // extrai DDL. Sem o CBDS, as tabelas USU_T* são lidas mesmo assim para aparecerem na lista do DDL.
    public async Task<IReadOnlyList<SeniorItemSummary>> ListItemsAsync(
        SeniorExportOptions options, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(options, listing: true, progress, cancellationToken);
        return data.Items(options);
    }

    private async Task<SeniorExportData> ReadAsync(
        SeniorExportOptions options, bool listing, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        var owners = await GetOwnersAsync(connection, cancellationToken);
        var owner = owners.FirstOrDefault(o => o == options.Owner)
            ?? throw new InvalidOperationException($"O schema {options.Owner} não tem as tabelas do Senior (R960SRV) ou não está acessível.");
        var schema = OracleExtensions.Quote(owner);

        var readTables = options.IncludeCbds || (listing && options.IncludeTableDdl);
        var extractDdl = options.IncludeTableDdl && !listing;

        // O total cresce com o número de tabelas do DDL, conhecido só depois de ler as tabelas.
        var totalSteps = (options.IncludeWebServices ? 1 : 0) + (readTables ? 1 : 0) + (options.IncludeCbds ? 3 : 0)
            + (options.IncludeRuleLinks ? 1 : 0) + (extractDdl && !readTables ? 1 : 0);
        var step = 0;
        void Report(string message) => progress?.Report(new ProgressInfo(++step, totalSteps, message));

        IReadOnlyList<SeniorService> services = [];
        IReadOnlyList<SeniorTable> tables = [];
        IReadOnlyList<SeniorField> standardFields = [];
        IReadOnlyList<SeniorList> lists = [];
        IReadOnlyList<SeniorView> views = [];
        IReadOnlyList<SeniorRuleLink> ruleLinks = [];
        IReadOnlyList<SeniorTableDdl> ddls = [];

        if (options.IncludeWebServices)
        {
            Report("Lendo webservices");
            services = await ReadServicesAsync(connection, schema, readRules: !listing, cancellationToken);
        }

        if (readTables)
        {
            Report("Lendo tabelas do CBDS");
            tables = await ReadTablesAsync(connection, schema, cancellationToken);
        }

        if (options.IncludeCbds)
        {
            Report("Lendo campos de usuário em tabelas padrão");
            standardFields = await ReadStandardTableFieldsAsync(connection, schema, cancellationToken);
            Report("Lendo listas do CBDS");
            lists = await ReadListsAsync(connection, schema, cancellationToken);
            Report("Lendo visões do CBDS");
            views = await ReadViewsAsync(connection, schema, cancellationToken);
        }

        if (options.IncludeRuleLinks)
        {
            Report("Lendo vínculos de regras");
            ruleLinks = await ReadRuleLinksAsync(connection, schema, cancellationToken);
        }

        if (extractDdl)
        {
            IReadOnlyList<string> tableNames;
            if (readTables)
            {
                tableNames = tables.Select(t => t.Name).ToList();
            }
            else
            {
                Report("Lendo nomes das tabelas do CBDS");
                tableNames = await ReadTableNamesAsync(connection, schema, cancellationToken);
            }

            tableNames = tableNames.Where(t => options.IsSelected(new SeniorItemKey(SeniorItemKind.Table, t))).ToList();
            totalSteps += tableNames.Count;
            ddls = await ReadTableDdlsAsync(connection, owner, tableNames, t => Report($"Extraindo DDL: {t}"), cancellationToken);
        }

        return new SeniorExportData(owner, services, tables, standardFields, lists, views, ruleLinks, ddls);
    }

    private static async Task<IReadOnlyList<string>> GetOwnersAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        const string sql = "SELECT OWNER FROM ALL_TABLES WHERE TABLE_NAME = 'R960SRV' ORDER BY OWNER";
        await using var command = connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var owners = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            owners.Add(reader.GetString(0));
        }

        return owners;
    }

    // Webservices -------------------------------------------------------------------------------------

    private async Task<IReadOnlyList<SeniorService>> ReadServicesAsync(
        OracleConnection connection, string schema, bool readRules, CancellationToken cancellationToken)
    {
        var serviceRows = await ReadRowsAsync(connection,
            $"""
            SELECT s.*
            FROM {schema}.R960SRV s
            WHERE s.SRVNAM NOT LIKE '{StandardServicePrefix}%'
            """, cancellationToken);

        var customServiceFilter =
            $"""
            EXISTS (SELECT 1
                    FROM {schema}.R960SRV s
                    WHERE s.PRVID = x.PRVID
                      AND s.SRVID = x.SRVID
                      AND s.SRVNAM NOT LIKE '{StandardServicePrefix}%')
            """;

        var portRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R960PRT x WHERE {customServiceFilter}", cancellationToken);
        List<SeniorRow> ruleRows = readRules
            ? await ReadRowsAsync(connection,
                $"SELECT x.PRVID, x.SRVID, x.PRTID, x.RULDAT FROM {schema}.R960RUL x WHERE {customServiceFilter}", cancellationToken)
            : [];

        var portsByService = portRows.ToLookup(p => p.Key("PRVID", "SRVID"));
        var rulesByPort = ruleRows.ToLookup(r => r.Key("PRVID", "SRVID", "PRTID"));

        var services = new List<SeniorService>();
        foreach (var serviceRow in serviceRows.OrderBy(s => s.Text("SRVNAM"), StringComparer.Ordinal))
        {
            var serviceName = serviceRow.Text("SRVNAM").Trim();
            var ports = new List<SeniorPort>();
            var servicePortRows = portsByService[serviceRow.Key("PRVID", "SRVID")].ToList();
            foreach (var portRow in servicePortRows.OrderBy(p => p.Text("PRTNAM"), StringComparer.Ordinal))
            {
                var portName = portRow.Text("PRTNAM").Trim();
                var parameters = portRow.Text("PRMDEF").Trim();
                var (ruleSource, ruleError) = DecodeRule(rulesByPort[portRow.Key("PRVID", "SRVID", "PRTID")].ToList());

                ports.Add(new SeniorPort(
                    serviceName,
                    portName,
                    SeniorColumns.ToAttributes(portRow, SeniorColumns.Port, "PRVID", "SRVID", "PRTID", "PRTNAM", "PRMDEF"),
                    parameters.Length == 0 ? null : RemoveChangeHistory(parameters),
                    ruleSource,
                    ruleError));
            }

            services.Add(new SeniorService(
                serviceName,
                SeniorColumns.ToAttributes(serviceRow, SeniorColumns.Service, "PRVID", "SRVID", "SRVNAM"),
                ports,
                SeniorColumns.LastChange([serviceRow, .. servicePortRows])));
        }

        return services;
    }

    private (string? Source, string? Error) DecodeRule(IReadOnlyList<SeniorRow> rules)
    {
        if (rules.Count == 0)
        {
            return (null, null);
        }

        if (rules.Count > 1)
        {
            return (null, $"{rules.Count} linhas em R960RUL para a mesma porta");
        }

        var result = decoder.Decode(rules[0].Bytes("RULDAT"));
        return (result.Source, result.Error);
    }

    // O histórico de alterações do PRMDEF muda a cada gravação e não interessa ao versionamento.
    private static string RemoveChangeHistory(string parameters) =>
        ChangeHistoryRegex().Replace(parameters.ReplaceLineEndings("\n"), "");

    // CBDS ---------------------------------------------------------------------------------------------

    private static async Task<IReadOnlyList<string>> ReadTableNamesAsync(OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        var rows = await ReadRowsAsync(connection,
            $"""SELECT t.TBLNAM FROM {schema}.R998TBL t WHERE UPPER(t.TBLNAM) LIKE 'USU\_T%' ESCAPE '\'""", cancellationToken);
        return rows.Select(r => r.Text("TBLNAM").Trim()).Order(StringComparer.Ordinal).ToList();
    }

    private static async Task<IReadOnlyList<SeniorTable>> ReadTablesAsync(OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        const string customTable = """UPPER(x.TBLNAM) LIKE 'USU\_T%' ESCAPE '\'""";
        var tableRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998TBL x WHERE {customTable}", cancellationToken);
        var fieldRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998FLD x WHERE {customTable}", cancellationToken);
        var indexRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998IND x WHERE {customTable}", cancellationToken);
        var indexFieldRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998FND x WHERE {customTable}", cancellationToken);

        var fieldsByTable = fieldRows.ToLookup(f => NameKey(f.Text("TBLNAM")));
        var indexesByTable = indexRows.ToLookup(i => NameKey(i.Text("TBLNAM")));
        var indexFieldsByIndex = indexFieldRows.ToLookup(f => NameKey(f.Text("TBLNAM")) + "|" + NameKey(f.Text("INDNAM")));
        var indexFieldOrder = new SeniorRowComparer(FirstSequenceColumn(indexFieldRows, "TBLNAM", "INDNAM", "FLDNAM"));

        var tables = new List<SeniorTable>();
        foreach (var tableRow in tableRows.OrderBy(t => t.Text("TBLNAM"), StringComparer.Ordinal))
        {
            var tableName = tableRow.Text("TBLNAM").Trim();
            var tableKey = NameKey(tableName);

            var tableFieldRows = fieldsByTable[tableKey].ToList();
            var tableIndexRows = indexesByTable[tableKey].ToList();
            var fields = tableFieldRows
                .Select(f => ToField(tableName, f))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(f => f.Name, StringComparer.Ordinal)
                .ToList();

            var indexes = new List<SeniorIndex>();
            foreach (var indexRow in tableIndexRows.OrderBy(i => i.Text("INDNAM"), StringComparer.Ordinal))
            {
                var indexName = indexRow.Text("INDNAM").Trim();
                var indexFields = indexFieldsByIndex[tableKey + "|" + NameKey(indexName)]
                    .Order(indexFieldOrder)
                    .Select(f => f.Text("FLDNAM").Trim())
                    .ToList();

                indexes.Add(new SeniorIndex(tableName, indexName,
                    SeniorColumns.ToAttributes(indexRow, SeniorColumns.Index, "TBLNAM", "INDNAM"), indexFields));
            }

            tables.Add(new SeniorTable(tableName, SeniorColumns.ToAttributes(tableRow, SeniorColumns.Table, "TBLNAM"), fields, indexes,
                SeniorColumns.LastChange([tableRow, .. tableFieldRows, .. tableIndexRows])));
        }

        return tables;
    }

    private static async Task<IReadOnlyList<SeniorField>> ReadStandardTableFieldsAsync(
        OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        var rows = await ReadRowsAsync(connection,
            $"""
            SELECT x.*
            FROM {schema}.R998FLD x
            WHERE UPPER(x.TBLNAM) NOT LIKE 'USU\_%' ESCAPE '\'
              AND UPPER(x.FLDNAM) LIKE 'USU\_%' ESCAPE '\'
            """, cancellationToken);

        return rows
            .Select(r => ToField(r.Text("TBLNAM").Trim(), r))
            .OrderBy(f => f.TableName, StringComparer.Ordinal)
            .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static SeniorField ToField(string tableName, SeniorRow row) =>
        new(tableName, row.Text("FLDNAM").Trim(), SeniorColumns.ToAttributes(row, SeniorColumns.Field, "TBLNAM", "FLDNAM"),
            SeniorColumns.LastChange([row]));

    private static async Task<IReadOnlyList<SeniorList>> ReadListsAsync(OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        const string customList = """UPPER(x.LSTNAM) LIKE 'USU\_%' ESCAPE '\'""";
        var listRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998LST x WHERE {customList}", cancellationToken);
        var itemRows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.R998LSF x WHERE {customList}", cancellationToken);
        var itemsByList = itemRows.ToLookup(i => NameKey(i.Text("LSTNAM")));
        var itemOrder = new SeniorRowComparer(FirstSequenceColumn(itemRows, "LSTNAM"));

        return listRows
            .OrderBy(l => l.Text("LSTNAM"), StringComparer.Ordinal)
            .Select(l =>
            {
                var items = itemsByList[NameKey(l.Text("LSTNAM"))].Order(itemOrder).ToList();
                return new SeniorList(
                    l.Text("LSTNAM").Trim(),
                    SeniorColumns.ToAttributes(l, SeniorColumns.List, "LSTNAM"),
                    items.Select(i => new SeniorRecord(SeniorColumns.ToAttributes(i, [], "LSTNAM"))).ToList(),
                    SeniorColumns.LastChange([l, .. items]));
            })
            .ToList();
    }

    private static async Task<IReadOnlyList<SeniorView>> ReadViewsAsync(OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        var rows = await ReadRowsAsync(connection,
            $"""SELECT x.* FROM {schema}.R998VIW x WHERE UPPER(x.VIWNAM) LIKE 'USU\_%' ESCAPE '\'""", cancellationToken);
        if (rows.Count == 0)
        {
            return [];
        }

        var textColumn = FindViewTextColumn(rows);
        var views = new List<SeniorView>();
        foreach (var group in rows.GroupBy(r => r.Text("VIWNAM").Trim()).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var parts = group.Order(new SeniorRowComparer("VIWSEQ")).ToList();
            var sql = textColumn is null ? "" : JoinViewText(parts.Select(p => p.Text(textColumn)).ToList());
            string[] excluded = textColumn is null ? ["VIWNAM", "VIWSEQ"] : ["VIWNAM", "VIWSEQ", textColumn];
            views.Add(new SeniorView(group.Key, SeniorColumns.ToAttributes(parts[0], [], excluded), sql, SeniorColumns.LastChange(parts)));
        }

        return views;
    }

    // O nome da coluna com o SQL da visão não é fixo entre versões: procura um nome conhecido e, se não
    // achar, usa a coluna de texto mais longa.
    private static string? FindViewTextColumn(IReadOnlyList<SeniorRow> rows)
    {
        var candidates = rows[0].Columns
            .Where(c => c is not ("VIWNAM" or "VIWSEQ") && !SeniorColumns.Volatile.Contains(c))
            .ToList();

        return candidates.FirstOrDefault(c => ViewTextColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
            ?? candidates.FirstOrDefault(c => c.Contains("SQL", StringComparison.OrdinalIgnoreCase))
            ?? candidates
                .Where(c => rows.Any(r => r[c] is string))
                .OrderByDescending(c => rows.Sum(r => r[c] is string text ? text.Length : 0))
                .FirstOrDefault();
    }

    // No banco, cada pedaço do SQL vem entre aspas. Se todos vierem assim, tira as aspas de cada pedaço;
    // senão, só as que envolvem o texto inteiro.
    private static string JoinViewText(IReadOnlyList<string> parts)
    {
        static bool IsQuoted(string text) => text.Length >= 2 && text[0] == '"' && text[^1] == '"';

        string text;
        if (parts.All(IsQuoted))
        {
            text = string.Concat(parts.Select(p => p[1..^1]));
        }
        else
        {
            text = string.Concat(parts);
            if (IsQuoted(text))
            {
                text = text[1..^1];
            }
        }

        return text.ReplaceLineEndings("\n").TrimEnd();
    }

    // Regras -------------------------------------------------------------------------------------------

    private static async Task<IReadOnlyList<SeniorRuleLink>> ReadRuleLinksAsync(OracleConnection connection, string schema, CancellationToken cancellationToken)
    {
        var rows = await ReadRowsAsync(connection, $"SELECT x.* FROM {schema}.E098REG x", cancellationToken);
        var linkOrder = new SeniorRowComparer("CODEMP", "CODTNS");

        var links = new List<SeniorRuleLink>();
        foreach (var group in rows
                     .GroupBy(r => (Module: r.Text("MODSIS").Trim(), Identifier: r.Text("IDEREG").Trim()))
                     .OrderBy(g => g.Key.Module, StringComparer.Ordinal)
                     .ThenBy(g => g.Key.Identifier, StringComparer.Ordinal))
        {
            var groupRows = group.Order(linkOrder).ToList();

            // O código da regra é o mesmo em todos os vínculos do identificador; fica no topo do arquivo.
            var header = new List<SeniorAttribute>();
            var excluded = new List<string> { "MODSIS", "IDEREG" };
            if (groupRows[0].Has("CODREG") && groupRows.Select(r => r.Text("CODREG")).Distinct().Count() == 1)
            {
                header.Add(new SeniorAttribute("codigo_regra", "CODREG", groupRows[0]["CODREG"]));
                excluded.Add("CODREG");
            }

            links.Add(new SeniorRuleLink(
                group.Key.Module,
                group.Key.Identifier,
                header,
                groupRows.Select(r => new SeniorRecord(SeniorColumns.ToAttributes(r, SeniorColumns.RuleLink, [.. excluded]))).ToList(),
                SeniorColumns.LastChange(groupRows)));
        }

        return links;
    }

    // DDL ----------------------------------------------------------------------------------------------

    private static async Task<IReadOnlyList<SeniorTableDdl>> ReadTableDdlsAsync(
        OracleConnection connection, string owner, IReadOnlyList<string> tableNames, Action<string> reportTable,
        CancellationToken cancellationToken)
    {
        if (tableNames.Count == 0)
        {
            return [];
        }

        // Sem os parâmetros de armazenamento (tablespace, storage), que variam entre ambientes. A
        // conexão volta ao pool, então os parâmetros da sessão são restaurados no final.
        const string setTransforms = """
            BEGIN
              DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'SEGMENT_ATTRIBUTES', FALSE);
              DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'SQLTERMINATOR', TRUE);
            END;
            """;
        const string resetTransforms = "BEGIN DBMS_METADATA.SET_TRANSFORM_PARAM(DBMS_METADATA.SESSION_TRANSFORM, 'DEFAULT'); END;";
        const string sql = "SELECT DBMS_METADATA.GET_DDL('TABLE', :tableName, :owner) FROM DUAL";

        await using (var command = connection.CreateTextCommand(setTransforms))
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var ddls = new List<SeniorTableDdl>();
        try
        {
            foreach (var tableName in tableNames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                reportTable(tableName);
                try
                {
                    await using var command = connection.CreateTextCommand(sql, ("tableName", tableName.ToUpperInvariant()), ("owner", owner));
                    command.InitialLOBFetchSize = -1;
                    var ddl = await command.ExecuteScalarAsync(cancellationToken) as string ?? "";
                    ddls.Add(new SeniorTableDdl(tableName, ddl.ReplaceLineEndings("\n").Trim('\n'), null));
                }
                catch (OracleException ex)
                {
                    ddls.Add(new SeniorTableDdl(tableName, null, ex.Message));
                }
            }
        }
        finally
        {
            await using var command = connection.CreateTextCommand(resetTransforms);
            await command.ExecuteNonQueryAsync(CancellationToken.None);
        }

        return ddls;
    }

    // Leitura genérica ---------------------------------------------------------------------------------

    private static async Task<List<SeniorRow>> ReadRowsAsync(OracleConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateTextCommand(sql);
        command.InitialLOBFetchSize = -1;
        command.InitialLONGFetchSize = -1;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var columns = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i).ToUpperInvariant()).ToArray();
        var rows = new List<SeniorRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var values = new object?[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                values[i] = ReadValue(reader, i);
            }

            rows.Add(new SeniorRow(columns, values));
        }

        return rows;
    }

    // Textos chegam já convertidos do charset do banco (ISO-8859-1); BLOBs voltam como bytes.
    private static object? ReadValue(OracleDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var type = reader.GetFieldType(ordinal);
        if (type == typeof(decimal))
        {
            var number = reader.GetOracleDecimal(ordinal);
            return decimal.TryParse(number.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value)
                ? value
                : number.ToString();
        }

        if (type == typeof(byte[]))
        {
            if (reader.GetDataTypeName(ordinal).Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                using var blob = reader.GetOracleBlob(ordinal);
                return blob.Value;
            }

            return (byte[])reader.GetValue(ordinal);
        }

        if (type == typeof(string))
        {
            return reader.GetString(ordinal);
        }

        return reader.GetValue(ordinal);
    }

    // Sequência dos itens (campos de índice, itens de lista): a primeira coluna numérica que não é chave.
    private static string[] FirstSequenceColumn(IReadOnlyList<SeniorRow> rows, params string[] keys)
    {
        if (rows.Count == 0)
        {
            return [];
        }

        var column = rows[0].Columns.FirstOrDefault(c =>
            !keys.Contains(c, StringComparer.OrdinalIgnoreCase) && !SeniorColumns.Volatile.Contains(c) && rows.Any(r => r[c] is decimal));
        return column is null ? [] : [column];
    }

    private static string NameKey(string name) => name.Trim().ToUpperInvariant();

    [GeneratedRegex(@"[ \t]*<changeHistory\b[^>]*/>\n?|[ \t]*<changeHistory\b.*?</changeHistory>\n?", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ChangeHistoryRegex();
}
