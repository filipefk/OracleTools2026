namespace OracleTools2026.Models;

public sealed record QueryResult(IReadOnlyList<string> Columns, IReadOnlyList<object?[]> Rows);
