namespace OracleTools2026.Models;

public sealed record SchedulerJob(
    DateTime? CreatedAt,
    DateTime? LastDdlAt,
    string Owner,
    string Name,
    string Action,
    bool Enabled,
    string JobType,
    string ProgramOwner,
    string ProgramName,
    string ScheduleOwner,
    string ScheduleName,
    string ScheduleType,
    string RepeatInterval,
    DateTime? LastStartDate,
    TimeSpan? LastRunDuration,
    DateTime? NextRunDate,
    long RunCount,
    long FailureCount,
    string Comments,
    string State,
    bool IsPastNextRunDate,
    bool StartedOverAnHourAgo)
{
    public string QualifiedName => $"{Owner}.{Name}";

    // Mesma classificação do original: jobs de execução única não são considerados atrasados nem demorados.
    public SchedulerJobHealth Health
    {
        get
        {
            if (!Enabled)
            {
                return SchedulerJobHealth.Disabled;
            }

            if (ScheduleType is "ONCE" or "NAMED" or "IMMEDIATE")
            {
                return SchedulerJobHealth.Ok;
            }

            return State == "RUNNING"
                ? StartedOverAnHourAgo ? SchedulerJobHealth.LongRunning : SchedulerJobHealth.Ok
                : IsPastNextRunDate ? SchedulerJobHealth.Overdue : SchedulerJobHealth.Ok;
        }
    }
}
