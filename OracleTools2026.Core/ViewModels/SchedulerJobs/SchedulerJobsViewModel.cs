using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.ViewModels.SchedulerJobs;

public sealed partial class SchedulerJobsViewModel : ToolViewModelBase
{
    private readonly SchedulerJobRepository _repository;
    private readonly DdlOutputService _output;

    public SchedulerJobsViewModel(ToolServices services, SchedulerJobRepository repository, DdlOutputService output)
        : base(services, "Jobs de banco")
    {
        _repository = repository;
        _output = output;
        Jobs.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Jobs.Count))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public ItemList<SchedulerJobItem> Jobs { get; } = new();

    public string CountText => $"{Jobs.Count} jobs listados";

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    protected override async Task OnConnectionChangedAsync()
    {
        Jobs.Clear();
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task RefreshAsync()
    {
        IReadOnlyList<SchedulerJob> jobs = [];
        var completed = await RunAsync("Listando jobs", async (_, ct) =>
            jobs = await Task.Run(() => _repository.GetJobsAsync(ct), ct));

        if (!completed)
        {
            return;
        }

        Jobs.Reset(jobs.Select(j => new SchedulerJobItem(j)));
        StatusText = $"{jobs.Count} jobs listados às {Now()}";
    }

    [RelayCommand]
    private async Task OpenActionAsync(SchedulerJobItem? item)
    {
        if (item?.Job is not { } job)
        {
            return;
        }

        if (job.Action.Length == 0)
        {
            ShowInfo($"O job {job.QualifiedName} não tem ação (usa o programa {job.ProgramOwner}.{job.ProgramName}).");
            return;
        }

        try
        {
            await _output.OpenInEditorAsync(job.Action, $"{job.QualifiedName}.sql");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private void CopyAction(SchedulerJobItem? item)
    {
        if (item?.Job.Action is { Length: > 0 } action)
        {
            Clipboard.SetText(action);
        }
    }
}
