using System.Text;

namespace OracleTools2026.Data.Senior;

// Uma linha lida com SELECT *: as tabelas do Senior variam entre versões, então só as colunas-chave
// são fixas no código e as demais são exportadas como vierem (veja SeniorColumns).
internal sealed class SeniorRow(IReadOnlyList<string> columns, object?[] values)
{
    private static readonly Encoding Latin1 = Encoding.Latin1;

    public IReadOnlyList<string> Columns { get; } = columns;

    public bool Has(string column) => IndexOf(column) >= 0;

    public object? this[string column] => IndexOf(column) is var index and >= 0 ? values[index] : null;

    public object? ValueAt(int index) => values[index];

    public string Text(string column) => this[column] switch
    {
        null => "",
        string text => text,
        byte[] bytes => Latin1.GetString(bytes),
        decimal number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        var other => other.ToString() ?? ""
    };

    public byte[]? Bytes(string column) => this[column] switch
    {
        byte[] bytes => bytes,
        string text => Latin1.GetBytes(text),
        _ => null
    };

    public string Key(params string[] keyColumns) => string.Join("|", keyColumns.Select(Text));

    private int IndexOf(string column)
    {
        for (var i = 0; i < Columns.Count; i++)
        {
            if (string.Equals(Columns[i], column, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
