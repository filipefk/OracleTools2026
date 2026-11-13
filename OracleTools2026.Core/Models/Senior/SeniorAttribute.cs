namespace OracleTools2026.Models.Senior;

// Uma coluna exportada: Key é o nome no YAML e Column, a coluna de origem no banco.
// Value é string, decimal, DateTime, lista de strings ou null.
public sealed record SeniorAttribute(string Key, string Column, object? Value);
