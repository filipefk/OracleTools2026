namespace OracleTools2026.Models.Senior;

// Error preenchido quando a DDL não foi gerada; o arquivo existente da tabela é mantido.
public sealed record SeniorTableDdl(string TableName, string? Ddl, string? Error);
