namespace OracleTools2026.Models.Senior;

// Resultado da gravação de um item; Message traz os erros do item (DDL, regra, nome repetido).
public sealed record SeniorItemProgress(SeniorItemKey Key, SeniorItemOutcome Outcome, string? Message);
