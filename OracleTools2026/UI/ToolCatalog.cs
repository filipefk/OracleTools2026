using OracleTools2026.Models;

namespace OracleTools2026.UI;

// Ferramentas disponíveis no menu inicial e na linha de comando (mesmos nomes do original).
// Os glifos são da fonte Segoe Fluent Icons / Segoe MDL2 Assets.
public static class ToolCatalog
{
    public const string ObjectsGroup = "Objetos de banco";
    public const string MonitoringGroup = "Monitoramento";

    public static IReadOnlyList<ToolDefinition> Tools { get; } =
    [
        new("EXTRACT_DDL_SOURCE", "Extrair DDL de objetos",
            "Lista objetos, últimas compilações e inválidos; extrai a DDL e recompila inválidos.",
            ObjectsGroup, "", typeof(ExtractDdlForm)),
        new("PROCURA_LOCK_BANCO", "Procurar lock de banco",
            "Executa periodicamente os scripts de lock e avisa quando encontrar.",
            MonitoringGroup, "", typeof(LockMonitorForm)),
        new("SESSOES", "Sessões ativas",
            "Lista as sessões com o nome real dos usuários e permite matá-las.",
            MonitoringGroup, "", typeof(SessionsForm)),
        new("JOBS_DE_BANCO", "Jobs de banco",
            "Jobs do DBMS_SCHEDULER: agenda, execuções, falhas e jobs travados.",
            MonitoringGroup, "", typeof(SchedulerJobsForm)),
        new("DBLINKS", "DBLINKs",
            "Lista os database links e testa a conectividade.",
            MonitoringGroup, "", typeof(DatabaseLinksForm))
    ];

    public static ToolDefinition? Find(string command) =>
        Tools.FirstOrDefault(t => t.Command.Equals(command.Trim(), StringComparison.OrdinalIgnoreCase));
}
