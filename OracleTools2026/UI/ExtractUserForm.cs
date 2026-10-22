using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;

namespace OracleTools2026.UI;

public partial class ExtractUserForm : DatabaseToolForm
{
    private const string AllOption = "Todos";

    private static readonly string[] OwnerFilterOptions = [AllOption, "Que seja owner", "Que não seja owner"];

    private readonly DatabaseUserRepository _repository;
    private readonly UserScriptGenerator _generator;
    private readonly DdlOutputService _output;
    private readonly IAppSettings _settings;
    private readonly ToolWindowManager _toolWindows;
    private readonly ListViewColumnSorter _sorter = new();

    private bool _updatingChecks;

    public ExtractUserForm(
        DatabaseToolDependencies dependencies,
        DatabaseUserRepository repository,
        UserScriptGenerator generator,
        DdlOutputService output,
        IAppSettings settings,
        ToolWindowManager toolWindows)
        : base(dependencies, "Extrair script de usuários")
    {
        _repository = repository;
        _generator = generator;
        _output = output;
        _settings = settings;
        _toolWindows = toolWindows;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
        usersListView.ListViewItemSorter = _sorter;
        ownerFilterComboBox.Items.AddRange(OwnerFilterOptions);
        ownerFilterComboBox.SelectedIndex = 0;
        ResetProfiles([]);
    }

    private async void ExtractUserForm_Load(object? sender, EventArgs e)
    {
        folderTextBox.Text = _settings.DefaultOutputFolder ?? "";
        customFilterTextBox.Text = _settings.UserSearchFilter ?? "";
        UpdateDestinationControls();
        UpdateListCount();

        if (Session.IsConnected)
        {
            await LoadProfilesAsync();
        }
    }

    // Os usuários listados e os profiles são do banco anterior. Os profiles são recarregados na próxima
    // listagem (ou ao abrir a lista de profiles), para não concorrer com a operação que pediu o login.
    protected override void OnConnectionChanged()
    {
        PopulateList([]);
        ResetProfiles([]);
    }

    protected override void OnBusyChanged(bool busy)
    {
        filterPanel.Enabled = !busy;
        extractButton.Enabled = !busy;
        selectAllCheckBox.Enabled = !busy;
        fileMenu.Enabled = !busy;
        cancelOperationButton.Enabled = busy;
    }

    #region Menu

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private void UserDetailsMenuItem_Click(object? sender, EventArgs e) => OpenUserDetails(SelectedUser?.Name);

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    #endregion

    #region Listagem

    private bool ProfilesLoaded => profileComboBox.Items.Count > 1;

    // Carrega os profiles ao abrir a lista (depois de trocar a conexão): fecha a lista, carrega e reabre.
    private void ProfileComboBox_DropDown(object? sender, EventArgs e)
    {
        if (ProfilesLoaded || !Session.IsConnected || IsBusy)
        {
            return;
        }

        BeginInvoke(new Action(async () =>
        {
            profileComboBox.DroppedDown = false;
            await LoadProfilesAsync();
            if (ProfilesLoaded)
            {
                profileComboBox.Focus();
                profileComboBox.DroppedDown = true;
            }
        }));
    }

    private async Task LoadProfilesAsync()
    {
        IReadOnlyList<string> profiles = [];
        if (await RunAsync("Listando profiles", async (_, ct) =>
                profiles = await Task.Run(() => _repository.GetProfilesAsync(ct), ct)))
        {
            ResetProfiles(profiles);
        }
    }

    private void ResetProfiles(IReadOnlyList<string> profiles)
    {
        var selected = profileComboBox.SelectedItem as string;
        profileComboBox.BeginUpdate();
        profileComboBox.Items.Clear();
        profileComboBox.Items.Add(AllOption);
        profileComboBox.Items.AddRange(profiles.Cast<object>().ToArray());
        profileComboBox.EndUpdate();
        profileComboBox.SelectedItem = selected is not null && profiles.Contains(selected) ? selected : AllOption;
    }

    private async void SearchButton_Click(object? sender, EventArgs e) => await SearchAsync();

    private async void FilterTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await SearchAsync();
        }
    }

    private async Task SearchAsync()
    {
        var customFilter = customFilterTextBox.Text.Trim();
        _settings.UserSearchFilter = customFilter;

        var criteria = new UserSearchCriteria(
            (UserOwnerFilter)Math.Max(0, ownerFilterComboBox.SelectedIndex),
            profileComboBox.SelectedItem is string profile && profile != AllOption ? profile : null,
            nameTextBox.Text.Trim(),
            customFilter);

        var profilesLoaded = ProfilesLoaded;
        IReadOnlyList<string> profiles = [];
        IReadOnlyList<DatabaseUser> users = [];
        var completed = await RunAsync("Listando usuários", async (_, ct) =>
        {
            if (!profilesLoaded)
            {
                profiles = await Task.Run(() => _repository.GetProfilesAsync(ct), ct);
            }

            users = await Task.Run(() => _repository.SearchAsync(criteria, ct), ct);
        });

        if (!completed)
        {
            return;
        }

        if (!profilesLoaded)
        {
            ResetProfiles(profiles);
        }

        PopulateList(users);
        SetStatus($"{users.Count} usuários listados às {DateTime.Now:HH:mm:ss}");
    }

    private void PopulateList(IReadOnlyList<DatabaseUser> users)
    {
        usersListView.BeginUpdate();
        try
        {
            _sorter.Reset();
            usersListView.Items.Clear();
            usersListView.Items.AddRange(users.Select(CreateListItem).ToArray());
        }
        finally
        {
            usersListView.EndUpdate();
        }

        usersListView.AutoResizeColumnsToContent();
        _updatingChecks = true;
        selectAllCheckBox.CheckState = CheckState.Unchecked;
        _updatingChecks = false;
        UpdateListCount();
    }

    private static ListViewItem CreateListItem(DatabaseUser user)
    {
        var item = new ListViewItem(user.Name) { Tag = user };
        item.SubItems.AddRange(new[]
        {
            ListViewExtensions.FormatDate(user.CreatedAt),
            user.Profile,
            user.Status,
            ListViewExtensions.FormatDate(user.LockDate),
            ListViewExtensions.FormatDate(user.ExpiryDate),
            ListViewExtensions.FormatDate(user.LastLogin)
        });

        if (user.Status.Contains("LOCKED", StringComparison.Ordinal))
        {
            item.ForeColor = SystemColors.GrayText;
        }

        return item;
    }

    private void UpdateListCount() =>
        listCountLabel.Text = $"{usersListView.Items.Count} usuários listados - {usersListView.CheckedItems.Count} usuários selecionados";

    private void UsersListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (_updatingChecks)
        {
            return;
        }

        _updatingChecks = true;
        var checkedCount = usersListView.CheckedItems.Count;
        selectAllCheckBox.CheckState = checkedCount switch
        {
            0 => CheckState.Unchecked,
            _ when checkedCount == usersListView.Items.Count => CheckState.Checked,
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
        usersListView.BeginUpdate();
        foreach (ListViewItem item in usersListView.Items)
        {
            item.Checked = selectAllCheckBox.Checked;
        }
        usersListView.EndUpdate();
        _updatingChecks = false;
        UpdateListCount();
    }

    private void UsersListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        usersListView.Sort();
    }

    private async void UsersListView_DoubleClick(object? sender, EventArgs e)
    {
        if (SelectedUser is { } user)
        {
            await ExtractSingleAsync(user.Name, toClipboard: false);
        }
    }

    private void UsersListView_KeyDown(object? sender, KeyEventArgs e) => usersListView.HandleCopyShortcuts(e);

    private DatabaseUser? SelectedUser =>
        usersListView.SelectedItems.Count == 1 ? usersListView.SelectedItems[0].Tag as DatabaseUser : null;

    private void UserContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        userContextMenu.Items.Clear();

        if (SelectedUser is { } user)
        {
            userContextMenu.Items.Add($"Ver script de criação do usuário {user.Name}", null,
                async (_, _) => await ExtractSingleAsync(user.Name, toClipboard: false));
            userContextMenu.Items.Add($"Copiar script de criação do usuário {user.Name}", null,
                async (_, _) => await ExtractSingleAsync(user.Name, toClipboard: true));
            userContextMenu.Items.Add($"Ver permissões do usuário {user.Name}", null, (_, _) => OpenUserDetails(user.Name));
            userContextMenu.Items.Add(new ToolStripSeparator());
        }

        if (usersListView.SelectedItems.Count > 0)
        {
            userContextMenu.Items.Add("Copiar linhas selecionadas", null, (_, _) => usersListView.CopySelectedToClipboard());
        }

        if (usersListView.Items.Count > 0)
        {
            userContextMenu.Items.Add("Copiar toda a lista", null, (_, _) => usersListView.CopyAllToClipboard());
            userContextMenu.Items.Add(new ToolStripSeparator());
        }

        userContextMenu.Items.Add("Informações do banco de dados", null, async (_, _) => await ShowServerInfoAsync());
        e.Cancel = IsBusy;
    }

    private void ManualListTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            manualListTextBox.SelectAll();
            e.SuppressKeyPress = true;
        }
    }

    private void OpenUserDetails(string? userName) => _toolWindows.Open<UserDetailsForm>(userName);

    #endregion

    #region Extração

    private void DestinationRadio_CheckedChanged(object? sender, EventArgs e) => UpdateDestinationControls();

    private void UpdateDestinationControls()
    {
        folderTextBox.Enabled = destinationFileRadio.Checked;
        browseFolderButton.Enabled = destinationFileRadio.Checked;
    }

    private void BrowseFolderButton_Click(object? sender, EventArgs e) => SelectFolder();

    private void CancelOperationButton_Click(object? sender, EventArgs e) => CancelOperation();

    private async void ExtractButton_Click(object? sender, EventArgs e)
    {
        var names = GetNamesToExtract();
        if (names is null)
        {
            return;
        }

        string? folder = null;
        if (destinationFileRadio.Checked)
        {
            folder = folderTextBox.Text.Trim();
            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                folder = SelectFolder();
                if (folder is null)
                {
                    return;
                }
            }

            _settings.DefaultOutputFolder = folder;
        }

        IReadOnlyList<UserScriptResult> results = [];
        var completed = await RunAsync("Extraindo scripts de usuários", async (progress, ct) =>
            results = await Task.Run(() => _generator.GenerateManyAsync(names, progress, ct), ct));

        if (!completed)
        {
            return;
        }

        var succeeded = results.Where(r => r.Succeeded).ToList();
        var failures = results.Where(r => !r.Succeeded).ToList();

        if (succeeded.Count > 0)
        {
            if (folder is not null)
            {
                await _output.SaveUserScriptsAsync(folder, succeeded);
            }
            else if (destinationClipboardRadio.Checked)
            {
                _output.CopyToClipboard(DdlOutputService.Combine(succeeded));
            }
            else
            {
                var fileName = succeeded.Count == 1 ? succeeded[0].FileName : $"Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                await _output.OpenInEditorAsync(DdlOutputService.Combine(succeeded), fileName);
            }
        }

        var summary = destinationClipboardRadio.Checked
            ? $"Script de {succeeded.Count} usuário(s) copiado para a área de transferência."
            : $"Extraídos {succeeded.Count} usuário(s)!";

        if (failures.Count > 0)
        {
            if (Ask($"{summary}\n{failures.Count} usuário(s) com erro. Deseja ver as mensagens de erro?"))
            {
                var errors = string.Join("\r\n", failures.Select(f => f.Error));
                await _output.OpenInEditorAsync(errors, "ErrosExtracaoUsuarios.txt");
            }
        }
        else if (!destinationEditorRadio.Checked)
        {
            ShowInfo(summary);
        }

        if (folder is not null && succeeded.Count > 0)
        {
            _output.OpenFolder(Path.Combine(folder, "USUARIOS"));
        }
    }

    private IReadOnlyList<string>? GetNamesToExtract()
    {
        if (sourceManualRadio.Checked)
        {
            var lines = manualListTextBox.Lines
                .Select(l => l.Trim().ToUpperInvariant())
                .Where(l => l.Length > 0)
                .Distinct()
                .ToList();

            if (lines.Count == 0)
            {
                ShowError("Nenhum usuário informado na lista.");
                return null;
            }

            var invalid = lines.Where(l => l.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0).ToList();
            if (invalid.Count > 0)
            {
                ShowError($"Lista de usuários inválida:\n{string.Join("\n", invalid.Take(10))}");
                return null;
            }

            return lines;
        }

        var items = sourceAllRadio.Checked
            ? usersListView.Items.Cast<ListViewItem>()
            : usersListView.CheckedItems.Cast<ListViewItem>();

        var names = items.Select(i => ((DatabaseUser)i.Tag!).Name).Distinct().ToList();
        if (names.Count == 0)
        {
            ShowError(sourceAllRadio.Checked ? "A lista está vazia." : "Nenhum usuário marcado.");
            return null;
        }

        return names;
    }

    private async Task ExtractSingleAsync(string userName, bool toClipboard)
    {
        UserScriptResult? result = null;
        await RunAsync($"Extraindo script do usuário {userName}", async (_, ct) =>
            result = await Task.Run(() => _generator.GenerateAsync(userName, ct), ct));

        if (result is null)
        {
            return;
        }

        if (!result.Succeeded)
        {
            ShowError(result.Error ?? "Script não gerado.");
        }
        else if (toClipboard)
        {
            _output.CopyToClipboard(result.Script);
            ShowInfo($"Script do usuário {userName} na área de transferência.");
        }
        else
        {
            await _output.OpenInEditorAsync(result.Script, result.FileName);
        }
    }

    private string? SelectFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Pasta onde os scripts serão gravados (na subpasta USUARIOS)",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(folderTextBox.Text) ? folderTextBox.Text : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        folderTextBox.Text = dialog.SelectedPath;
        _settings.DefaultOutputFolder = dialog.SelectedPath;
        return dialog.SelectedPath;
    }

    #endregion
}
