namespace OracleTools2026.Models.Senior;

public sealed record SeniorDependencyScan(IReadOnlyList<SeniorDependency> Dependencies, IReadOnlyList<string> SecretWarnings);
