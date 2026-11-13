using System.Globalization;

namespace OracleTools2026.Data.Senior;

// Ordem estável das linhas para o diff no Git: primeiro as colunas indicadas, depois todas as outras
// na ordem do banco (números comparados como números, textos por ordinal).
internal sealed class SeniorRowComparer(params string[] priority) : IComparer<SeniorRow>
{
    public int Compare(SeniorRow? x, SeniorRow? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null || y is null)
        {
            return x is null ? -1 : 1;
        }

        foreach (var column in priority.Concat(x.Columns.Except(priority, StringComparer.OrdinalIgnoreCase)))
        {
            if (SeniorColumns.Volatile.Contains(column))
            {
                continue;
            }

            var result = CompareValues(x[column], y[column]);
            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }

    private static int CompareValues(object? x, object? y) => (x, y) switch
    {
        (null, null) => 0,
        (null, _) => -1,
        (_, null) => 1,
        (decimal a, decimal b) => a.CompareTo(b),
        (DateTime a, DateTime b) => a.CompareTo(b),
        (byte[] a, byte[] b) => a.AsSpan().SequenceCompareTo(b),
        _ => string.CompareOrdinal(Convert.ToString(x, CultureInfo.InvariantCulture), Convert.ToString(y, CultureInfo.InvariantCulture))
    };
}
