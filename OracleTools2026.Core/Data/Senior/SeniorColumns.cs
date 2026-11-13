using OracleTools2026.Models.Senior;

namespace OracleTools2026.Data.Senior;

// Nome no YAML de cada coluna das tabelas do Senior, na ordem em que aparecem no arquivo (igual às
// amostras do repositório). Colunas que não estão aqui são exportadas depois, com o próprio nome em
// minúsculas, na ordem do banco; assim uma coluna nova numa versão do Senior aparece no diff.
internal static class SeniorColumns
{
    // Mudam a cada gravação no Senior e só gerariam ruído no diff.
    public static readonly IReadOnlySet<string> Volatile =
        new HashSet<string>(["PRMDTM", "LASUPD", "CODUSU", "USUGER", "DATGER", "HORGER"], StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlyList<(string Column, string Key)> Service =
    [
        ("SRVATV", "ativo"),
        ("SRVURL", "url")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> Port =
    [
        ("PRTDES", "descricao"),
        ("TIPSER", "tipo_servico"),
        ("PRTRUL", "usa_regra"),
        ("PRTATV", "ativa"),
        ("PRTAUT", "autenticacao"),
        ("FLGLOG", "grava_log"),
        ("RESMOD", "modo_resposta"),
        ("PRTURI", "uri"),
        ("PRTACT", "action"),
        ("PRTENC", "encoding"),
        ("PRTXNM", "xml_namespace")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> Table =
    [
        ("TBLDES", "descricao"),
        ("ORINAM", "nome_origem"),
        ("OBJSIS", "objeto_sistema"),
        ("PKFLDS", "chave_primaria")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> Field =
    [
        ("DATTYP", "tipo"),
        ("LSTNAM", "lista")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> Index =
    [
        ("INDUNI", "unico")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> List =
    [
        ("DATTYP", "tipo")
    ];

    public static readonly IReadOnlyList<(string Column, string Key)> RuleLink =
    [
        ("CODEMP", "empresa"),
        ("CODTNS", "transacao")
    ];

    // Colunas guardadas como texto separado por vírgula que viram lista no YAML.
    private static readonly IReadOnlySet<string> ListColumns = new HashSet<string>(["PKFLDS"], StringComparer.OrdinalIgnoreCase);

    private static readonly DateTime NullDate = new(1900, 12, 31, 23, 59, 59);

    public static IReadOnlyList<SeniorAttribute> ToAttributes(
        SeniorRow row, IReadOnlyList<(string Column, string Key)> mapping, params string[] excluded)
    {
        var skip = new HashSet<string>(excluded, StringComparer.OrdinalIgnoreCase);
        var attributes = new List<SeniorAttribute>();

        foreach (var (column, key) in mapping)
        {
            if (row.Has(column) && !skip.Contains(column))
            {
                attributes.Add(new SeniorAttribute(key, column, ConvertValue(column, row[column])));
                skip.Add(column);
            }
        }

        foreach (var column in row.Columns)
        {
            if (!skip.Contains(column) && !Volatile.Contains(column))
            {
                attributes.Add(new SeniorAttribute(column.ToLowerInvariant(), column, ConvertValue(column, row[column])));
            }
        }

        return attributes;
    }

    // Data da última alteração das linhas: a maior entre LASUPD, PRMDTM e DATGER + HORGER (minutos desde a
    // meia-noite, como o Senior grava horas). Datas até 31/12/1900 são a "data nula" do Senior.
    public static DateTime? LastChange(IEnumerable<SeniorRow> rows)
    {
        DateTime? latest = null;
        foreach (var row in rows)
        {
            foreach (var date in RowDates(row))
            {
                if (date > NullDate && (latest is null || date > latest))
                {
                    latest = date;
                }
            }
        }

        return latest;
    }

    private static IEnumerable<DateTime> RowDates(SeniorRow row)
    {
        if (row["LASUPD"] is DateTime lastUpdate)
        {
            yield return lastUpdate;
        }

        if (row["PRMDTM"] is DateTime parametersDate)
        {
            yield return parametersDate;
        }

        if (row["DATGER"] is DateTime generated)
        {
            yield return row["HORGER"] is decimal minutes and >= 0 and < 1440
                ? generated.Date.AddMinutes((double)minutes)
                : generated;
        }
    }

    private static object? ConvertValue(string column, object? value)
    {
        if (value is byte[] bytes)
        {
            value = System.Text.Encoding.Latin1.GetString(bytes);
        }

        if (value is string text)
        {
            text = text.TrimEnd();
            if (ListColumns.Contains(column))
            {
                return text.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            return text.ReplaceLineEndings("\n");
        }

        return value;
    }
}
