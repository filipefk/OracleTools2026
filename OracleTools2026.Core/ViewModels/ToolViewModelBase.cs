using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using System.Reflection;

namespace OracleTools2026.ViewModels;

// Base das ferramentas que trabalham com uma conexão: título com os dados do banco, login sob demanda,
// operações em segundo plano com progresso e cancelamento, informações do banco e mensagens.
public abstract partial class ToolViewModelBase : ObservableObject
{
    protected const string ReadyText = "Pronto";
    private const string CanceledText = "Operação cancelada";
    private const string NotConnectedText = "NÃO CONECTADO";

    private readonly SynchronizationContext? _uiContext;
    private IToolHost? _host;
    private CancellationTokenSource? _operationCancellation;
    private bool _connectionChangedWhileBusy;
    private bool _activated;
    private IReadOnlyList<IRelayCommand>? _commands;

    protected ToolViewModelBase(ToolServices services, string toolTitle)
    {
        Services = services;
        ToolTitle = toolTitle;
        _uiContext = SynchronizationContext.Current;
        Title = $"{toolTitle} - {NotConnectedText}";
        ServerInfo = NotConnectedText;
        StatusText = ReadyText;
        ProgressMaximum = 1;
        NoticeMessage = "";
    }

    public string ToolTitle { get; }

    [ObservableProperty]
    public partial string Title { get; private set; }

    [ObservableProperty]
    public partial string ServerInfo { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    public partial bool IsBusy { get; private set; }

    public bool IsIdle => !IsBusy;

    [ObservableProperty]
    public partial string StatusText { get; set; }

    [ObservableProperty]
    public partial bool IsProgressIndeterminate { get; private set; }

    [ObservableProperty]
    public partial double ProgressValue { get; private set; }

    [ObservableProperty]
    public partial double ProgressMaximum { get; private set; }

    [ObservableProperty]
    public partial bool IsNoticeOpen { get; set; }

    [ObservableProperty]
    public partial NoticeKind NoticeKind { get; private set; }

    [ObservableProperty]
    public partial string NoticeMessage { get; private set; }

    protected ToolServices Services { get; }

    protected IOracleSession Session => Services.Session;

    protected IClipboardService Clipboard => Services.Clipboard;

    protected IToolHost Host => _host ?? throw new InvalidOperationException("A tela não foi associada a uma janela.");

    protected bool IsActivated => _activated;

    public void AttachHost(IToolHost host) => _host = host;

    // Chamado pela tela na primeira vez que ela é exibida.
    public async Task ActivateAsync()
    {
        if (_activated)
        {
            return;
        }

        _activated = true;
        Session.ConnectionChanged += Session_ConnectionChanged;
        await RefreshTitleAsync();
        await OnActivatedAsync();
    }

    // Chamado quando a janela da ferramenta é fechada.
    public void Deactivate()
    {
        _operationCancellation?.Cancel();
        if (_activated)
        {
            Session.ConnectionChanged -= Session_ConnectionChanged;
            _activated = false;
        }

        OnDeactivated();
    }

    protected virtual Task OnActivatedAsync() => Task.CompletedTask;

    protected virtual void OnDeactivated()
    {
    }

    // A conexão é compartilhada por todas as telas do processo; chamado quando ela é trocada (por esta ou
    // por outra tela) para descartar/recarregar os dados do banco anterior. Se houver uma operação em
    // andamento, é chamado só quando ela terminar.
    protected virtual Task OnConnectionChangedAsync() => Task.CompletedTask;

    partial void OnIsBusyChanged(bool value) => RefreshCommands();

    // Os comandos usam IsIdle/IsBusy como CanExecute; avisa todos quando o estado muda.
    protected void RefreshCommands()
    {
        foreach (var command in _commands ??= FindCommands())
        {
            command.NotifyCanExecuteChanged();
        }
    }

    private IReadOnlyList<IRelayCommand> FindCommands() =>
        GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => typeof(IRelayCommand).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0)
            .Select(p => p.GetValue(this))
            .OfType<IRelayCommand>()
            .ToList();

    private void Session_ConnectionChanged(object? sender, EventArgs e)
    {
        if (_uiContext is not null && SynchronizationContext.Current != _uiContext)
        {
            _uiContext.Post(_ => Session_ConnectionChanged(sender, e), null);
            return;
        }

        _ = HandleConnectionChangedAsync();
    }

    private async Task HandleConnectionChangedAsync()
    {
        if (!_activated)
        {
            return;
        }

        if (IsBusy)
        {
            _connectionChangedWhileBusy = true;
        }
        else
        {
            await OnConnectionChangedAsync();
        }

        await RefreshTitleAsync();
    }

    private async Task RefreshTitleAsync()
    {
        if (!Session.IsConnected)
        {
            ServerInfo = NotConnectedText;
        }
        else
        {
            try
            {
                ServerInfo = await Task.Run(() => Services.Repository.GetServerInfoAsync());
            }
            catch (Exception)
            {
                ServerInfo = Session.Current?.DisplayName ?? "";
            }
        }

        Title = $"{ToolTitle} - {ServerInfo}";
    }

    protected Task<bool> ShowLoginAsync() => Host.ShowLoginAsync();

    protected async Task<bool> EnsureConnectedAsync() => Session.IsConnected || await ShowLoginAsync();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task ConnectAsync() => OnConnectRequestedAsync();

    protected virtual async Task OnConnectRequestedAsync() => await ShowLoginAsync();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ShowServerInfoAsync()
    {
        IReadOnlyList<DatabaseProperty> details = [];
        var completed = await RunAsync("Buscando informações do banco", async (_, ct) =>
            details = await Task.Run(() => Services.Repository.GetDatabaseDetailsAsync(ct), ct));

        if (completed)
        {
            await Host.ShowDatabaseInfoAsync(details);
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel()
    {
        if (_operationCancellation is { } cancellation)
        {
            cancellation.Cancel();
            StatusText = "Cancelando...";
        }
    }

    // requiresConnection = false para operações que não usam o banco (ex.: consulta ao Active Directory).
    protected async Task<bool> RunAsync(string description, Func<IProgress<ProgressInfo>, CancellationToken, Task> operation,
        bool requiresConnection = true)
    {
        // O login dispara ConnectionChanged, que pode iniciar outra operação na tela enquanto o diálogo
        // ainda está aberto; por isso IsBusy é conferido de novo depois dele.
        if (IsBusy || (requiresConnection && !await EnsureConnectedAsync()) || IsBusy)
        {
            return false;
        }

        using var cancellation = new CancellationTokenSource();
        _operationCancellation = cancellation;
        SetBusy(true, description);

        var progress = new Progress<ProgressInfo>(p =>
        {
            if (_operationCancellation is null)
            {
                return;
            }

            IsProgressIndeterminate = false;
            ProgressMaximum = Math.Max(1, p.Total);
            ProgressValue = Math.Min(p.Current, ProgressMaximum);
            StatusText = $"{p.Current} de {p.Total} - {p.Message}";
        });

        try
        {
            await operation(progress, cancellation.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            StatusText = CanceledText;
            return false;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return false;
        }
        finally
        {
            _operationCancellation = null;
            SetBusy(false, null);

            if (_connectionChangedWhileBusy && _activated)
            {
                _connectionChangedWhileBusy = false;
                _uiContext?.Post(_ => _ = OnConnectionChangedAsync(), null);
            }
        }
    }

    private void SetBusy(bool busy, string? description)
    {
        IsProgressIndeterminate = busy;
        ProgressValue = 0;

        if (busy)
        {
            StatusText = description ?? "";
        }
        else if (StatusText != CanceledText)
        {
            StatusText = ReadyText;
        }

        IsBusy = busy;
    }

    protected Task<bool> AskAsync(string message) => Host.AskAsync(ToolTitle, message);

    protected Task ShowMessageAsync(string title, string message) => Host.ShowMessageAsync(title, message);

    // Avisos não bloqueantes, mostrados numa InfoBar da tela.
    protected void ShowInfo(string message) => ShowNotice(NoticeKind.Informational, message);

    protected void ShowSuccess(string message) => ShowNotice(NoticeKind.Success, message);

    protected void ShowWarning(string message) => ShowNotice(NoticeKind.Warning, message);

    protected void ShowError(string message) => ShowNotice(NoticeKind.Error, message);

    protected void HideNotice() => IsNoticeOpen = false;

    private void ShowNotice(NoticeKind kind, string message)
    {
        // Fecha antes para a InfoBar reaparecer mesmo que a mensagem anterior ainda esteja visível.
        IsNoticeOpen = false;
        NoticeKind = kind;
        NoticeMessage = message;
        IsNoticeOpen = true;
    }

    protected static string Now() => DateTime.Now.ToString("HH:mm:ss");
}
