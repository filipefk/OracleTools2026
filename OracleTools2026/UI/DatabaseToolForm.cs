using OracleTools2026.Data;
using OracleTools2026.Models;

namespace OracleTools2026.UI;

// Base das ferramentas que trabalham com uma conexão: título com os dados do banco, login sob demanda,
// operações em segundo plano com progresso e cancelamento, informações do banco e mensagens.
public class DatabaseToolForm : Form
{
    private const string CanceledText = "Operação cancelada";
    private const string ReadyText = "Pronto";

    private readonly DatabaseToolDependencies? _dependencies;
    private readonly string _toolTitle = "";
    private CancellationTokenSource? _operationCancellation;
    private ToolStripStatusLabel? _statusLabel;
    private ToolStripProgressBar? _progressBar;
    private bool _connectionChangedWhileBusy;

    // Usado apenas pelo designer do Visual Studio.
    public DatabaseToolForm()
    {
    }

    protected DatabaseToolForm(DatabaseToolDependencies dependencies, string toolTitle)
    {
        _dependencies = dependencies;
        _toolTitle = toolTitle;
        AppIcon.ApplyTo(this);
    }

    protected IOracleSession Session => Dependencies.Session;

    protected string ToolTitle => _toolTitle;

    protected bool IsBusy => _operationCancellation is not null;

    private DatabaseToolDependencies Dependencies =>
        _dependencies ?? throw new InvalidOperationException("Formulário criado sem dependências.");

    protected void BindStatus(ToolStripStatusLabel statusLabel, ToolStripProgressBar progressBar)
    {
        _statusLabel = statusLabel;
        _progressBar = progressBar;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_dependencies is null)
        {
            return;
        }

        Session.ConnectionChanged += Session_ConnectionChanged;
        await RefreshTitleAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _operationCancellation?.Cancel();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (_dependencies is not null)
        {
            Session.ConnectionChanged -= Session_ConnectionChanged;
        }

        base.OnFormClosed(e);
    }

    // A conexão é compartilhada por todas as telas do processo; chamado quando ela é trocada (por esta ou
    // por outra tela) para descartar/recarregar os dados do banco anterior. Se houver uma operação em
    // andamento, é chamado só quando ela terminar.
    protected virtual void OnConnectionChanged()
    {
    }

    // Habilita/desabilita os controles da tela enquanto uma operação está em andamento.
    protected virtual void OnBusyChanged(bool busy)
    {
    }

    private async void Session_ConnectionChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => Session_ConnectionChanged(sender, e));
            return;
        }

        if (IsBusy)
        {
            _connectionChangedWhileBusy = true;
        }
        else
        {
            OnConnectionChanged();
        }

        await RefreshTitleAsync();
    }

    private async Task RefreshTitleAsync()
    {
        if (!Session.IsConnected)
        {
            Text = $"{_toolTitle} - NÃO CONECTADO";
            return;
        }

        try
        {
            var serverInfo = await Task.Run(() => Dependencies.Repository.GetServerInfoAsync());
            Text = $"{_toolTitle} - {serverInfo}";
        }
        catch (Exception)
        {
            Text = $"{_toolTitle} - {Session.Current?.User}@{Session.Current?.DataSource}";
        }
    }

    protected bool ShowLogin()
    {
        using var form = Dependencies.LoginFormFactory();
        return form.ShowDialog(this) == DialogResult.OK;
    }

    protected bool EnsureConnected() => Session.IsConnected || ShowLogin();

    protected async Task ShowServerInfoAsync()
    {
        IReadOnlyList<DatabaseProperty> details = [];
        var completed = await RunAsync("Buscando informações do banco", async (_, ct) =>
            details = await Task.Run(() => Dependencies.Repository.GetDatabaseDetailsAsync(ct), ct));

        if (completed)
        {
            using var form = Dependencies.DatabaseInfoFormFactory();
            form.ShowDetails(details);
            form.ShowDialog(this);
        }
    }

    // requiresConnection = false para operações que não usam o banco (ex.: consulta ao Active Directory).
    protected async Task<bool> RunAsync(string description, Func<IProgress<ProgressInfo>, CancellationToken, Task> operation,
        bool requiresConnection = true)
    {
        // O login (EnsureConnected) dispara ConnectionChanged, que pode iniciar outra operação na tela
        // enquanto o diálogo ainda está aberto; por isso IsBusy é conferido de novo depois dele.
        if (IsBusy || (requiresConnection && !EnsureConnected()) || IsBusy)
        {
            return false;
        }

        using var cancellation = new CancellationTokenSource();
        _operationCancellation = cancellation;
        SetBusy(true, description);

        var progress = new Progress<ProgressInfo>(p =>
        {
            if (_operationCancellation is null || _progressBar is null)
            {
                return;
            }

            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Maximum = Math.Max(1, p.Total);
            _progressBar.Value = Math.Min(p.Current, _progressBar.Maximum);
            SetStatus($"{p.Current} de {p.Total} - {p.Message}");
        });

        try
        {
            await operation(progress, cancellation.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            SetStatus(CanceledText);
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

            if (_connectionChangedWhileBusy && IsHandleCreated && !IsDisposed)
            {
                _connectionChangedWhileBusy = false;
                BeginInvoke(OnConnectionChanged);
            }
        }
    }

    protected void CancelOperation()
    {
        if (_operationCancellation is { } cancellation)
        {
            cancellation.Cancel();
            SetStatus("Cancelando...");
        }
    }

    protected void SetStatus(string? text)
    {
        if (_statusLabel is not null)
        {
            _statusLabel.Text = text;
        }
    }

    private void SetBusy(bool busy, string? description)
    {
        UseWaitCursor = busy;
        if (_progressBar is not null)
        {
            _progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            _progressBar.Value = 0;
        }

        if (busy)
        {
            SetStatus(description);
        }
        else if (_statusLabel?.Text != CanceledText)
        {
            SetStatus(ReadyText);
        }

        OnBusyChanged(busy);
    }

    protected bool Ask(string message) =>
        MessageBox.Show(this, message, _toolTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    protected void ShowInfo(string message) =>
        MessageBox.Show(this, message, _toolTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

    protected void ShowError(string message) =>
        MessageBox.Show(this, message, _toolTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
