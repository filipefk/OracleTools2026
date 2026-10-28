using OracleTools2026.Formatting;
using OracleTools2026.Models;

namespace OracleTools2026.ViewModels.SchedulerJobs;

public sealed class SchedulerJobItem(SchedulerJob job)
{
    public SchedulerJob Job { get; } = job;

    public DateTime? CreatedAt => Job.CreatedAt;
    public string CreatedAtText => DisplayFormat.Date(Job.CreatedAt);
    public DateTime? LastDdlAt => Job.LastDdlAt;
    public string LastDdlAtText => DisplayFormat.Date(Job.LastDdlAt);
    public string Owner => Job.Owner;
    public string Name => Job.Name;
    public string ActionText => Job.Action.ReplaceLineEndings(" ");
    public string EnabledText => Job.Enabled ? "TRUE" : "FALSE";
    public string JobType => Job.JobType;
    public string ProgramOwner => Job.ProgramOwner;
    public string ProgramName => Job.ProgramName;
    public string ScheduleOwner => Job.ScheduleOwner;
    public string ScheduleName => Job.ScheduleName;
    public string ScheduleType => Job.ScheduleType;
    public string RepeatInterval => Job.RepeatInterval;
    public DateTime? LastStartDate => Job.LastStartDate;
    public string LastStartDateText => DisplayFormat.Date(Job.LastStartDate);
    public TimeSpan? LastRunDuration => Job.LastRunDuration;
    public string LastRunDurationText => DisplayFormat.Duration(Job.LastRunDuration);
    public DateTime? NextRunDate => Job.NextRunDate;
    public string NextRunDateText => DisplayFormat.Date(Job.NextRunDate);
    public long RunCount => Job.RunCount;
    public long FailureCount => Job.FailureCount;
    public string Comments => Job.Comments;
    public string State => Job.State;
    public SchedulerJobHealth Health => Job.Health;

    public string HealthText => Job.Health switch
    {
        SchedulerJobHealth.Disabled => "Desabilitado",
        SchedulerJobHealth.Overdue => "Atrasado (possivelmente travado)",
        SchedulerJobHealth.LongRunning => "Rodando há mais de 1 hora",
        _ => "OK"
    };
}
