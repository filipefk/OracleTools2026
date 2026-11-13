namespace OracleTools2026.Models.Senior;

// Files: caminhos completos que fazem parte da exportação (gravados ou mantidos por erro no item).
public sealed record SeniorWriteResult(
    IReadOnlyCollection<string> Files,
    int Written,
    int Unchanged,
    IReadOnlyList<string> Errors,
    IReadOnlyList<SeniorItemProgress> ItemOutcomes);
