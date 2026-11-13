namespace OracleTools2026.Models.Senior;

// Calls: serviços chamados ("serviço/porta" quando o serviço é exportado; com o flag de padrão Senior).
public sealed record SeniorDependency(string Source, IReadOnlyList<SeniorServiceCall> Calls, IReadOnlyList<string> Tables);
