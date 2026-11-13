namespace OracleTools2026.Models.Senior;

public sealed record SeniorExportSummary(
    int Services,
    int Ports,
    int Tables,
    int StandardTables,
    int Lists,
    int Views,
    int RuleLinks,
    int TableDdls,
    int FilesWritten,
    int FilesDeleted,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> SecretWarnings);
