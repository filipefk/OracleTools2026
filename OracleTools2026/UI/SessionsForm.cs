using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.UI;

public partial class SessionsForm : DatabaseToolForm
{
    private readonly SessionRepository _repository;
    private readonly RealNameDirectory _realNames;
    private readonly ListViewColumnSorter _sorter = new();

    private bool _updatingChecks;

    public SessionsForm(DatabaseToolDependencies dependencies, SessionRepository repository, RealNameDirectory realNames)
        : base(dependencies, "Sessões")
    {
        _repository = repository;
        _realNames = realNames;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
        sessionsListView.ListViewItemSorter = _sorter;
    }

    private async void SessionsForm_Load(object? sender, EventArgs e)
    {
        UpdateListCount();
        if (Session.IsConnected)
        {
            await RefreshSessionsAsync();
        }
    }

    // A lista do banco anterior é descartada: matar sessões dela usaria SID/SERIAL# de outro banco.
    protected override async void OnConnectionChanged()
    {
        PopulateList([], new Dictionary<string, string>());
        if (Session.IsConnected)
        {
            await RefreshSessionsAsync();
        }
    }

    protected override void OnBusyChanged(bool busy)
    {
        refreshButton.Enabled = !busy;
        killButton.Enabled = !busy;
        selectAllCheckBox.Enabled = !busy;
        fileMenu.Enabled = !busy;
        cancelButton.Enabled = busy;
    }

    #region Menu e botões

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    private async void RefreshButton_Click(object? sender, EventArgs e) => await RefreshSessionsAsync();

    private async void KillButton_Click(object? sender, EventArgs e) => await KillSessionsAsync();

    private void CancelButton_Click(object? sender, EventArgs e) => CancelOperation();

    #endregion

    #region Lista de sessões

    private async Task RefreshSessionsAsync()
    {
        IReadOnlyList<DatabaseSession> sessions = [];
        IReadOnlyDictionary<string, string> names = new Dictionary<string, string>();
        string? namesError = null;

        var completed = await RunAsync("Listando sessões", async (_, ct) =>
        {
            sessions = await Task.Run(() => _repository.GetSessionsAsync(ct), ct);

            // Como no original, só procura o nome real de sessões de usuários (as de background não têm USERNAME).
            var logins = sessions.Where(s => s.UserName.Length > 0).SelectMany(s => new[] { s.UserName, s.OsUser });
            try
            {
                names = await Task.Run(() => _realNames.ResolveAsync(logins, ct), ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                namesError = ex.Message;
            }
        });

        if (!completed)
        {
            return;
        }

        PopulateList(sessions, names);
        SetStatus(namesError is null
            ? $"{sessions.Count} sessões listadas às {DateTime.Now:HH:mm:ss}"
            : $"{sessions.Count} sessões listadas - não foi possível buscar os nomes reais: {namesError}");
    }

    private void PopulateList(IReadOnlyList<DatabaseSession> sessions, IReadOnlyDictionary<string, string> names)
    {
        _updatingChecks = true;
        sessionsListView.BeginUpdate();
        try
        {
            _sorter.Reset();
            sessionsListView.Items.Clear();
            sessionsListView.Items.AddRange(sessions.Select(s => CreateListItem(s, names)).ToArray());
            selectAllCheckBox.CheckState = CheckState.Unchecked;
        }
        finally
        {
            sessionsListView.EndUpdate();
            _updatingChecks = false;
        }

        sessionsListView.AutoResizeColumnsToContent();
        UpdateListCount();
    }

    private static ListViewItem CreateListItem(DatabaseSession session, IReadOnlyDictionary<string, string> names)
    {
        var userRealName = names.GetValueOrDefault(session.UserName, "");
        var osUserRealName = names.GetValueOrDefault(session.OsUser, "");

        var item = new ListViewItem(session.InstanceId.ToString()) { Tag = session };
        item.SubItems.AddRange(new[]
        {
            session.Sid.ToString(),
            session.SerialNumber.ToString(),
            session.UserName,
            userRealName,
            session.OsUser,
            osUserRealName,
            session.Machine,
            session.Status,
            session.Program,
            session.Type,
            session.Module,
            ListViewExtensions.FormatDate(session.LogonTime),
            session.State
        });

        if (userRealName.Length > 0 && osUserRealName.Length > 0 && !RealNameDirectory.AreSameName(userRealName, osUserRealName))
        {
            // Usuário do banco e usuário do SO pertencem a pessoas diferentes.
            item.BackColor = Color.LightGray;
        }

        return item;
    }

    private void UpdateListCount() =>
        listCountLabel.Text = $"{sessionsListView.Items.Count} sessões listadas - {sessionsListView.CheckedItems.Count} marcadas";

    private void SessionsListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (_updatingChecks)
        {
            return;
        }

        _updatingChecks = true;
        var checkedCount = sessionsListView.CheckedItems.Count;
        selectAllCheckBox.CheckState = checkedCount switch
        {
            0 => CheckState.Unchecked,
            _ when checkedCount == sessionsListView.Items.Count => CheckState.Checked,
            _ => CheckState.Indeterminate
        };
        _updatingChecks = false;
        UpdateListCount();
    }

    private void SelectAllCheckBox_CheckStateChanged(object? sender, EventArgs e)
    {
        if (_updatingChecks || selectAllCheckBox.CheckState == CheckState.Indeterminate)
        {
            return;
        }

        _updatingChecks = true;
        sessionsListView.BeginUpdate();
        foreach (ListViewItem item in sessionsListView.Items)
        {
            item.Checked = selectAllCheckBox.Checked;
        }
        sessionsListView.EndUpdate();
        _updatingChecks = false;
        UpdateListCount();
    }

    private void SessionsListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        sessionsListView.Sort();
    }

    private void SessionsListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && !IsBusy)
        {
            e.SuppressKeyPress = true;
            killButton.PerformClick();
            return;
        }

        sessionsListView.HandleCopyShortcuts(e);
    }

    private void SessionContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        sessionContextMenu.Items.Clear();

        var targets = GetKillTargets();
        if (targets.Count > 0)
        {
            var text = targets.Count == 1 ? "Matar a sessão selecionada" : $"Matar as {targets.Count} sessões selecionadas";
            sessionContextMenu.Items.Add(text, null, async (_, _) => await KillSessionsAsync());
            sessionContextMenu.Items.Add(new ToolStripSeparator());
        }

        if (sessionsListView.SelectedItems.Count > 0)
        {
            sessionContextMenu.Items.Add("Copiar linhas selecionadas", null, (_, _) => sessionsListView.CopySelectedToClipboard());
        }

        if (sessionsListView.Items.Count > 0)
        {
            sessionContextMenu.Items.Add("Copiar toda a lista", null, (_, _) => sessionsListView.CopyAllToClipboard());
            sessionContextMenu.Items.Add(new ToolStripSeparator());
        }

        sessionContextMenu.Items.Add("Informações do banco de dados", null, async (_, _) => await ShowServerInfoAsync());
        e.Cancel = IsBusy;
    }

    #endregion

    #region Matar sessões

    // Como no original, considera as sessões selecionadas e as marcadas.
    private List<DatabaseSession> GetKillTargets() =>
        sessionsListView.SelectedItems.Cast<ListViewItem>()
            .Concat(sessionsListView.CheckedItems.Cast<ListViewItem>())
            .Select(i => (DatabaseSession)i.Tag!)
            .Distinct()
            .ToList();

    private async Task KillSessionsAsync()
    {
        var targets = GetKillTargets();
        if (targets.Count == 0)
        {
            ShowInfo("Marque ou selecione as sessões que deseja matar.");
            return;
        }

        var description = targets.Count == 1
            ? $"a sessão {targets[0].Sid},{targets[0].SerialNumber} do usuário {targets[0].UserName} ({targets[0].OsUser} em {targets[0].Machine})"
            : $"as {targets.Count} sessões selecionadas";
        if (!Ask($"Confirma matar {description}?"))
        {
            return;
        }

        IReadOnlyList<SessionKillResult> results = [];
        var completed = await RunAsync("Matando sessões", async (progress, ct) =>
            results = await Task.Run(() => _repository.KillAsync(targets, progress, ct), ct));

        if (!completed)
        {
            return;
        }

        var failures = results.Where(r => !r.Succeeded).ToList();
        var message = failures.Count == 0
            ? "Comando executado."
            : $"{results.Count - failures.Count} sessão(ões) desconectada(s) e {failures.Count} com erro:\n\n" +
              string.Join("\n", failures.Take(10).Select(f => $"{f.Session.Sid},{f.Session.SerialNumber}: {f.Error}"));

        if (Ask($"{message}\n\nDeseja atualizar a lista de sessões?"))
        {
            await RefreshSessionsAsync();
        }
    }

    #endregion
}
