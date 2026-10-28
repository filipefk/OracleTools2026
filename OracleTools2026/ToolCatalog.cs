using OracleTools2026.Models;
using OracleTools2026.Presentation;
using OracleTools2026.Views.Tools;

namespace OracleTools2026;

// Ferramentas disponíveis no menu e na linha de comando (mesmos nomes do original).
// Os glifos são da fonte Segoe Fluent Icons.
public static class ToolCatalog
{
    public const string ObjectsGroup = "Objetos de banco";
    public const string UsersGroup = "Usuários";
    public const string MonitoringGroup = "Monitoramento";

    public static IReadOnlyList<ToolDefinition> Tools { get; } =
    [
        new(ToolCommands.ExtractDdl, "Extrair DDL de objetos",
            "Lista objetos, últimas compilações e inválidos; extrai a DDL e recompila inválidos.",
            ObjectsGroup, "\uE943", typeof(ExtractDdlPage)),
        new(ToolCommands.ExtractUser, "Extrair script de usuários",
            "Lista usuários por owner, profile e nome e gera o script de criação com cotas, roles e grants.",
            UsersGroup, "\uE77B", typeof(ExtractUserPage)),
        new(ToolCommands.UserDetails, "Detalhes de um usuário",
            "Propriedades, profile, cotas, privilégios, roles e grants; busca o usuário no Active Directory.",
            UsersGroup, "\uE779", typeof(UserDetailsPage)),
        new(ToolCommands.LockMonitor, "Procurar lock de banco",
            "Executa periodicamente os scripts de lock e avisa quando encontrar.",
            MonitoringGroup, "\uE72E", typeof(LockMonitorPage)),
        new(ToolCommands.Sessions, "Sessões ativas",
            "Lista as sessões com o nome real dos usuários e permite matá-las.",
            MonitoringGroup, "\uE716", typeof(SessionsPage)),
        new(ToolCommands.SchedulerJobs, "Jobs de banco",
            "Jobs do DBMS_SCHEDULER: agenda, execuções, falhas e jobs travados.",
            MonitoringGroup, "\uE823", typeof(SchedulerJobsPage)),
        new(ToolCommands.DatabaseLinks, "DBLINKs",
            "Lista os database links e testa a conectividade.",
            MonitoringGroup, "\uE71B", typeof(DatabaseLinksPage))
    ];

    public static ToolDefinition? Find(string command) =>
        Tools.FirstOrDefault(t => t.Command.Equals(command.Trim(), StringComparison.OrdinalIgnoreCase));

    public static ToolDefinition? Find(Type pageType) =>
        Tools.FirstOrDefault(t => t.PageType == pageType);
}
