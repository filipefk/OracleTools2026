using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Formatting;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.ViewModels.LockMonitor;

public sealed partial class LockMonitorViewModel : ToolViewModelBase
{
    private readonly ScriptRunner _scriptRunner;
    private readonly LockScriptStore _scripts;
    private CancellationTokenSource? _monitorCancellation;
    private int _activeScriptIndex = -1;

    public LockMonitorViewModel(ToolServices services, ScriptRunner scriptRunner, LockScriptStore scripts)
        : base(services, "Procura Lock no banco")
    {
        _scriptRunner = scriptRunner;
        _scripts = scripts;
        Columns = [];
        IntervalSeconds = 5;
        NotifyWhenFound = true;
        MonitorStateText = "Parado";
    }

    public ItemList<QueryRow> Rows { get; } = new();

    // Nomes das colunas do último resultado; a tela recria as colunas quando eles mudam.
    [ObservableProperty]
    public partial IReadOnlyList<string> Columns { get; private set; }

    [ObservableProperty]
    public partial double IntervalSeconds { get; set; }

    [ObservableProperty]
    public partial bool NotifyWhenFound { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    public partial bool IsMonitoring { get; private set; }

    [ObservableProperty]
    public partial string MonitorStateText { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScript1Command))]
    public partial bool IsScript1Available { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScript2Command))]
    public partial bool IsScript2Available { get; private set; }

    protected override Task OnActivatedAsync()
    {
        UpdateScriptAvailability();
        if (!IsScript1Available && !IsScript2Available)
        {
            ShowError($"Nenhum script de lock encontrado. Procurando por {string.Join(" e ", LockScriptStore.FileNames)} nas pastas:\n\n" +
                      string.Join("\n", AppFiles.SearchFolders));
        }

        return Task.CompletedTask;
    }

    // O monitoramento ativo continua, já usando a nova conexão no próximo ciclo.
    protected override Task OnConnectionChangedAsync()
    {
        Rows.Clear();
        return Task.CompletedTask;
    }

    protected override void OnDeactivated() => StopMonitoring();

    protected override async Task OnConnectRequestedAsync()
    {
        StopMonitoring();
        await ShowLoginAsync();
    }

    [RelayCommand(CanExecute = nameof(IsScript1Available), AllowConcurrentExecutions = true)]
    private Task StartScript1Async() => StartMonitoringAsync(0);

    [RelayCommand(CanExecute = nameof(IsScript2Available), AllowConcurrentExecutions = true)]
    private Task StartScript2Async() => StartMonitoringAsync(1);

    [RelayCommand(CanExecute = nameof(IsMonitoring))]
    private void Stop() => StopMonitoring();

    [RelayCommand]
    private async Task CopyScriptAsync(int scriptIndex)
    {
        try
        {
            if (await _scripts.LoadAsync(scriptIndex) is { Length: > 0 } script)
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

    private async Task StartMonitoringAsync(int scriptIndex)
    {
        StopMonitoring();
        if (!await EnsureConnectedAsync())
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
            UpdateScriptAvailability();
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
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(double.IsNaN(IntervalSeconds) ? 5 : IntervalSeconds, 1, 3600)),
                    cancellation.Token);
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
        StatusText = "Procurando...";
        try
        {
            var result = await Task.Run(() => _scriptRunner.ExecuteQueryAsync(script, cancellationToken), cancellationToken);
            ShowResult(result);
            StatusText = $"Última verificação às {Now()} - {result.Rows.Count} linha(s) encontrada(s)";

            if (NotifyWhenFound && result.Rows.Count > 0)
            {
                Host.BringToFront();
                SystemSound.Beep();
            }
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        catch (Exception ex)
        {
            // Como no original, avisa o erro e continua procurando no próximo intervalo.
            StatusText = $"Erro na verificação às {Now()}";
            ShowError($"Erro ao buscar lock de banco:\n\n{ex.Message}");
        }
    }

    private void ShowResult(QueryResult result)
    {
        if (!Columns.SequenceEqual(result.Columns))
        {
            Rows.Clear();
            Columns = result.Columns.ToList();
        }

        Rows.Reset(result.Rows.Select(row => new QueryRow(row, row.Select(DisplayFormat.Value).ToList())));
    }

    private void UpdateScriptAvailability()
    {
        IsScript1Available = _scripts.Exists(0);
        IsScript2Available = _scripts.Exists(1);
    }

    private void UpdateMonitorState()
    {
        IsMonitoring = _monitorCancellation is not null;
        MonitorStateText = IsMonitoring ? $"Ativado (Script {_activeScriptIndex + 1})" : "Parado";
    }
}
