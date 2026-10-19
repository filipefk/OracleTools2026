using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using System.Media;

namespace OracleTools2026.UI;

public partial class LockMonitorForm : DatabaseToolForm
{
    private readonly ScriptRunner _scriptRunner;
    private readonly LockScriptStore _scripts;
    private readonly ListViewColumnSorter _sorter = new();

    private CancellationTokenSource? _monitorCancellation;
    private int _activeScriptIndex = -1;

    public LockMonitorForm(DatabaseToolDependencies dependencies, ScriptRunner scriptRunner, LockScriptStore scripts)
        : base(dependencies, "Procura Lock no banco")
    {
        _scriptRunner = scriptRunner;
        _scripts = scripts;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
        locksListView.ListViewItemSorter = _sorter;
    }

    private bool IsMonitoring => _monitorCancellation is not null;

    private void LockMonitorForm_Load(object? sender, EventArgs e)
    {
        UpdateScriptButtons();
        UpdateMonitorState();

        if (!script1Button.Enabled && !script2Button.Enabled)
        {
            ShowError($"Nenhum script de lock encontrado. Procurando por {string.Join(" e ", LockScriptStore.FileNames)} nas pastas:\n\n" +
                      string.Join("\n", AppFiles.SearchFolders));
        }
    }

    // O monitoramento ativo continua, já usando a nova conexão no próximo ciclo.
    protected override void OnConnectionChanged() => locksListView.Items.Clear();

    private void LockMonitorForm_FormClosing(object? sender, FormClosingEventArgs e) => StopMonitoring();

    #region Menu

    private void ConnectMenuItem_Click(object? sender, EventArgs e)
    {
        StopMonitoring();
        ShowLogin();
    }

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    #endregion

    #region Monitoramento

    private async void Script1Button_Click(object? sender, EventArgs e) => await StartMonitoringAsync(0);

    private async void Script2Button_Click(object? sender, EventArgs e) => await StartMonitoringAsync(1);

    private void StopButton_Click(object? sender, EventArgs e) => StopMonitoring();

    private async Task StartMonitoringAsync(int scriptIndex)
    {
        StopMonitoring();
        if (!EnsureConnected())
        {
            return;
        }

        string? script;
        try
        {
            // Relido a cada início, para considerar alterações feitas no arquivo com a tela aberta.
            script = await _scripts.LoadAsync(scriptIndex);
        }
        catch (Exception ex)
        {
            ShowError($"Erro ao ler o script {LockScriptStore.FileNames[scriptIndex]}:\n{ex.Message}");
            return;
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            ShowError($"Script {LockScriptStore.FileNames[scriptIndex]} não encontrado ou vazio.");
            UpdateScriptButtons();
            return;
        }

        using var cancellation = new CancellationTokenSource();
        _monitorCancellation = cancellation;
        _activeScriptIndex = scriptIndex;
        UpdateMonitorState();

        try
        {
            while (true)
            {
                await SearchLocksAsync(script, cancellation.Token);
                await Task.Delay(TimeSpan.FromSeconds((double)intervalNumeric.Value), cancellation.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Monitoramento parado pelo usuário.
        }
        finally
        {
            if (_monitorCancellation == cancellation)
            {
                _monitorCancellation = null;
                UpdateMonitorState();
            }
        }
    }

    private void StopMonitoring()
    {
        if (_monitorCancellation is { } cancellation)
        {
            _monitorCancellation = null;
            cancellation.Cancel();
        }

        UpdateMonitorState();
    }

    private async Task SearchLocksAsync(string script, CancellationToken cancellationToken)
    {
        SetStatus("Procurando...");
        try
        {
            var result = await Task.Run(() => _scriptRunner.ExecuteQueryAsync(script, cancellationToken), cancellationToken);
            ShowResult(result);
            SetStatus($"Última verificação às {DateTime.Now:HH:mm:ss} - {result.Rows.Count} linha(s) encontrada(s)");

            if (notifyCheckBox.Checked && result.Rows.Count > 0)
            {
                NotifyLockFound();
            }
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        catch (Exception ex)
        {
            // Como no original, avisa o erro e continua procurando no próximo intervalo.
            SetStatus($"Erro na verificação às {DateTime.Now:HH:mm:ss}");
            ShowError($"Erro ao buscar lock de banco:\n\n{ex.Message}");
        }
    }

    private void ShowResult(QueryResult result)
    {
        locksListView.BeginUpdate();
        try
        {
            var sameColumns = locksListView.Columns.Count == result.Columns.Count &&
                              locksListView.Columns.Cast<ColumnHeader>().Select(c => c.Text).SequenceEqual(result.Columns);
            if (!sameColumns)
            {
                _sorter.Reset();
                locksListView.Columns.Clear();
                foreach (var column in result.Columns)
                {
                    locksListView.Columns.Add(column);
                }
            }

            locksListView.Items.Clear();
            locksListView.Items.AddRange(result.Rows
                .Select(row => new ListViewItem(row.Select(ListViewExtensions.FormatValue).ToArray()))
                .ToArray());
        }
        finally
        {
            locksListView.EndUpdate();
        }

        locksListView.AutoResizeColumnsToContent();
    }

    private void NotifyLockFound()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        TopMost = true;
        TopMost = false;
        Activate();
        SystemSounds.Beep.Play();
    }

    private void UpdateScriptButtons()
    {
        script1Button.Enabled = _scripts.Exists(0);
        script2Button.Enabled = _scripts.Exists(1);
    }

    private void UpdateMonitorState()
    {
        stopButton.Enabled = IsMonitoring;
        stateLabel.Text = IsMonitoring ? $"Ativado (Script {_activeScriptIndex + 1})" : "Parado";
        stateLabel.ForeColor = IsMonitoring ? Color.DarkGreen : SystemColors.GrayText;
    }

    #endregion

    #region Scripts e lista

    private async void CopyScriptMenuItem_Click(object? sender, EventArgs e)
    {
        var index = scriptContextMenu.SourceControl == script2Button ? 1 : 0;
        try
        {
            if (await _scripts.LoadAsync(index) is { Length: > 0 } script)
            {
                Clipboard.SetText(script);
                ShowInfo("Script na área de transferência.");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void LocksListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        locksListView.Sort();
    }

    private void LocksListView_KeyDown(object? sender, KeyEventArgs e) => locksListView.HandleCopyShortcuts(e);

    private void ListContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        copySelectedMenuItem.Enabled = locksListView.SelectedItems.Count > 0;
        copyAllMenuItem.Enabled = locksListView.Items.Count > 0;
    }

    private void CopySelectedMenuItem_Click(object? sender, EventArgs e) => locksListView.CopySelectedToClipboard();

    private void CopyAllMenuItem_Click(object? sender, EventArgs e) => locksListView.CopyAllToClipboard();

    private async void ListServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    #endregion
}
