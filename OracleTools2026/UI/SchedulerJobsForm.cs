using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.UI;

public partial class SchedulerJobsForm : DatabaseToolForm
{
    private readonly SchedulerJobRepository _repository;
    private readonly DdlOutputService _output;
    private readonly ListViewColumnSorter _sorter = new();

    public SchedulerJobsForm(DatabaseToolDependencies dependencies, SchedulerJobRepository repository, DdlOutputService output)
        : base(dependencies, "Jobs de banco")
    {
        _repository = repository;
        _output = output;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
        jobsListView.ListViewItemSorter = _sorter;
    }

    private SchedulerJob? SelectedJob =>
        jobsListView.SelectedItems.Count == 1 ? jobsListView.SelectedItems[0].Tag as SchedulerJob : null;

    private async void SchedulerJobsForm_Load(object? sender, EventArgs e)
    {
        UpdateListCount();
        if (Session.IsConnected)
        {
            await RefreshJobsAsync();
        }
    }

    protected override async void OnConnectionChanged()
    {
        PopulateList([]);
        if (Session.IsConnected)
        {
            await RefreshJobsAsync();
        }
    }

    protected override void OnBusyChanged(bool busy)
    {
        refreshButton.Enabled = !busy;
        fileMenu.Enabled = !busy;
        cancelButton.Enabled = busy;
    }

    #region Menu e botões

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    private async void RefreshButton_Click(object? sender, EventArgs e) => await RefreshJobsAsync();

    private void CancelButton_Click(object? sender, EventArgs e) => CancelOperation();

    #endregion

    #region Lista de jobs

    private async Task RefreshJobsAsync()
    {
        IReadOnlyList<SchedulerJob> jobs = [];
        var completed = await RunAsync("Listando jobs", async (_, ct) =>
            jobs = await Task.Run(() => _repository.GetJobsAsync(ct), ct));

        if (!completed)
        {
            return;
        }

        PopulateList(jobs);
        SetStatus($"{jobs.Count} jobs listados às {DateTime.Now:HH:mm:ss}");
    }

    private void PopulateList(IReadOnlyList<SchedulerJob> jobs)
    {
        jobsListView.BeginUpdate();
        try
        {
            _sorter.Reset();
            jobsListView.Items.Clear();
            jobsListView.Items.AddRange(jobs.Select(CreateListItem).ToArray());
        }
        finally
        {
            jobsListView.EndUpdate();
        }

        jobsListView.AutoResizeColumnsToContent();
        UpdateListCount();
    }

    private static ListViewItem CreateListItem(SchedulerJob job)
    {
        var item = new ListViewItem(ListViewExtensions.FormatDate(job.CreatedAt)) { Tag = job };
        item.SubItems.AddRange(new[]
        {
            ListViewExtensions.FormatDate(job.LastDdlAt),
            job.Owner,
            job.Name,
            job.Action.ReplaceLineEndings(" "),
            job.Enabled ? "TRUE" : "FALSE",
            job.JobType,
            job.ProgramOwner,
            job.ProgramName,
            job.ScheduleOwner,
            job.ScheduleName,
            job.ScheduleType,
            job.RepeatInterval,
            ListViewExtensions.FormatDate(job.LastStartDate),
            ListViewExtensions.FormatDuration(job.LastRunDuration),
            ListViewExtensions.FormatDate(job.NextRunDate),
            job.RunCount.ToString(),
            job.FailureCount.ToString(),
            job.Comments,
            job.State,
            GetHealthText(job.Health)
        });

        item.ForeColor = job.Health switch
        {
            SchedulerJobHealth.Disabled => Color.Silver,
            SchedulerJobHealth.Overdue => Color.Red,
            SchedulerJobHealth.LongRunning => Color.DarkOrange,
            _ => item.ForeColor
        };

        return item;
    }

    private static string GetHealthText(SchedulerJobHealth health) => health switch
    {
        SchedulerJobHealth.Disabled => "Desabilitado",
        SchedulerJobHealth.Overdue => "Atrasado (possivelmente travado)",
        SchedulerJobHealth.LongRunning => "Rodando há mais de 1 hora",
        _ => "OK"
    };

    private void UpdateListCount() =>
        listCountLabel.Text = $"{jobsListView.Items.Count} jobs listados";

    private void JobsListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        jobsListView.Sort();
    }

    private void JobsListView_KeyDown(object? sender, KeyEventArgs e) => jobsListView.HandleCopyShortcuts(e);

    private async void JobsListView_DoubleClick(object? sender, EventArgs e)
    {
        if (SelectedJob is { } job)
        {
            await OpenActionAsync(job);
        }
    }

    private void JobContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        jobContextMenu.Items.Clear();

        if (SelectedJob is { } job)
        {
            jobContextMenu.Items.Add($"Abrir ação de {job.QualifiedName} no editor", null, async (_, _) => await OpenActionAsync(job));
            jobContextMenu.Items.Add($"Copiar ação de {job.QualifiedName}", null, (_, _) => CopyAction(job));
            jobContextMenu.Items.Add(new ToolStripSeparator());
        }

        if (jobsListView.SelectedItems.Count > 0)
        {
            jobContextMenu.Items.Add("Copiar linhas selecionadas", null, (_, _) => jobsListView.CopySelectedToClipboard());
        }

        if (jobsListView.Items.Count > 0)
        {
            jobContextMenu.Items.Add("Copiar toda a lista", null, (_, _) => jobsListView.CopyAllToClipboard());
            jobContextMenu.Items.Add(new ToolStripSeparator());
        }

        jobContextMenu.Items.Add("Informações do banco de dados", null, async (_, _) => await ShowServerInfoAsync());
        e.Cancel = IsBusy;
    }

    private async Task OpenActionAsync(SchedulerJob job)
    {
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

    private void CopyAction(SchedulerJob job)
    {
        if (job.Action.Length > 0)
        {
            _output.CopyToClipboard(job.Action);
        }
    }

    #endregion
}
