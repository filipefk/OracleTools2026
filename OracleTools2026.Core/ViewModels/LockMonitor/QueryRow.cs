namespace OracleTools2026.ViewModels.LockMonitor;

// Linha de uma consulta genérica; as colunas da lista são ligadas a Values[i].
public sealed class QueryRow(IReadOnlyList<object?> rawValues, List<string> values)
{
    public IReadOnlyList<object?> RawValues { get; } = rawValues;

    public List<string> Values { get; } = values;
}
