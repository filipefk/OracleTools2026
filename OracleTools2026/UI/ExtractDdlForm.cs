using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Text;

namespace OracleTools2026.UI;

public partial class ExtractDdlForm : Form
{
    private const string AllOption = "Todos";
    private const string BaseTitle = "Extract DDL Oracle";

    private readonly IOracleSession _session;
    private readonly DatabaseObjectRepository _repository;
    private readonly DdlExtractor _extractor;
    private readonly InvalidObjectRecompiler _recompiler;
    private readonly DdlOutputService _output;
    private readonly IAppSettings _settings;
    private readonly Func<LoginForm> _loginFormFactory;
    private readonly Func<SettingsForm> _settingsFormFactory;
    private readonly Func<DatabaseInfoForm> _databaseInfoFormFactory;
    private readonly ListViewColumnSorter _sorter = new();

    private CancellationTokenSource? _operationCancellation;
    private bool _connectionChangedWhileBusy;
    private bool _updatingChecks;

    public ExtractDdlForm(
        IOracleSession session,
        DatabaseObjectRepository repository,
        DdlExtractor extractor,
        InvalidObjectRecompiler recompiler,
        DdlOutputService output,
        IAppSettings settings,
        Func<LoginForm> loginFormFactory,
        Func<SettingsForm> settingsFormFactory,
        Func<DatabaseInfoForm> databaseInfoFormFactory)
    {
        _session = session;
        _repository = repository;
        _extractor = extractor;
        _recompiler = recompiler;
        _output = output;
        _settings = settings;
        _loginFormFactory = loginFormFactory;
        _settingsFormFactory = settingsFormFactory;
        _databaseInfoFormFactory = databaseInfoFormFactory;

        InitializeComponent();

        objectsListView.ListViewItemSorter = _sorter;
        typeComboBox.Items.Add(AllOption);
        typeComboBox.Items.AddRange(DatabaseObjectRepository.SupportedTypes.Cast<object>().ToArray());
        typeComboBox.SelectedIndex = 0;

        _session.ConnectionChanged += Session_ConnectionChanged;
    }

    private bool IsBusy => _operationCancellation is not null;

    private DdlExtractionOptions ExtractionOptions => new(includeHeaderCheckBox.Checked, inlineErrorsCheckBox.Checked);

    #region Form

    private async void ExtractDdlForm_Load(object? sender, EventArgs e)
    {
        includeHeaderCheckBox.Checked = _settings.IncludeHeaderOnDdlExtraction;
        inlineErrorsCheckBox.Checked = _settings.InlineCompileErrorsOnDdlExtraction;
        folderTextBox.Text = _settings.DefaultOutputFolder ?? "";
        UpdateDestinationControls();
        UpdateListCount();

        if (_session.IsConnected)
        {
            await RefreshConnectionInfoAsync();
        }
        else
        {
            Text = $"{BaseTitle} - NÃO CONECTADO";
        }
    }

    private void ExtractDdlForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _operationCancellation?.Cancel();
        _session.ConnectionChanged -= Session_ConnectionChanged;
    }

    private async void Session_ConnectionChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Session_ConnectionChanged(sender, e)));
            return;
        }

        // A conexão é compartilhada com as outras telas. Os objetos listados são do banco anterior e não
        // podem ser extraídos/recompilados no novo; com uma operação em andamento, descarta ao final dela.
        if (IsBusy)
        {
            _connectionChangedWhileBusy = true;
            return;
        }

        PopulateList([]);
        await RefreshConnectionInfoAsync();
    }

    #endregion

    #region Menu

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private void SettingsMenuItem_Click(object? sender, EventArgs e)
    {
        using var form = _settingsFormFactory();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            includeHeaderCheckBox.Checked = _settings.IncludeHeaderOnDdlExtraction;
            inlineErrorsCheckBox.Checked = _settings.InlineCompileErrorsOnDdlExtraction;
            folderTextBox.Text = _settings.DefaultOutputFolder ?? "";
        }
    }

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    #endregion

    #region Listagens

    private void NameTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            searchButton.PerformClick();
        }
    }

    private async void SearchButton_Click(object? sender, EventArgs e)
    {
        var owner = NullIfAll(ownerComboBox.Text);
        var type = NullIfAll(typeComboBox.Text);
        var name = nameTextBox.Text;

        await RunAsync("Listando objetos", async (_, ct) =>
        {
            var objects = await Task.Run(() => _repository.SearchAsync(owner, type, name, ct), ct);
            PopulateList(objects);
            if (objects.Count == 0)
            {
                ShowInfo("Nenhum objeto encontrado!");
            }
        });
    }

    private async void RecentButton_Click(object? sender, EventArgs e)
    {
        var days = (int)daysNumeric.Value;
        ownerComboBox.Text = "";
        typeComboBox.SelectedIndex = 0;
        nameTextBox.Text = "";

        await RunAsync($"Listando objetos compilados nos últimos {days} dias", async (_, ct) =>
        {
            var objects = await Task.Run(() => _repository.GetRecentlyCompiledAsync(days, ct), ct);
            PopulateList(objects);
            if (objects.Count == 0)
            {
                ShowInfo($"Nenhum objeto compilado nos últimos {days} dias.");
            }
        });
    }

    private async void InvalidButton_Click(object? sender, EventArgs e) => await ListInvalidAsync(showEmptyMessage: true);

    private async Task ListInvalidAsync(bool showEmptyMessage)
    {
        await RunAsync("Listando objetos inválidos", async (_, ct) =>
        {
            var objects = await Task.Run(() => _repository.GetInvalidAsync(ct), ct);
            PopulateList(objects);
            if (objects.Count == 0 && showEmptyMessage)
            {
                ShowInfo("Nenhum objeto inválido.");
            }
        });
    }

    private async void RecompileButton_Click(object? sender, EventArgs e)
    {
        var answer = MessageBox.Show(this, "Recompilar todos os objetos inválidos do banco?", "Recompilar inválidos",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        RecompileResult? result = null;
        await RunAsync("Recompilando objetos inválidos", async (progress, ct) =>
            result = await Task.Run(() => _recompiler.RecompileAsync(progress, ct), ct));

        if (result is null)
        {
            return;
        }

        if (result.AnyRecompiled && Ask("A quantidade de objetos inválidos mudou. Deseja atualizar a lista de objetos inválidos?"))
        {
            await ListInvalidAsync(showEmptyMessage: true);
        }

        if (Ask("Deseja visualizar o relatório de compilações?"))
        {
            await _output.OpenInEditorAsync(result.Report, "RelatorioRecompilacao.txt");
        }
    }

    #endregion

    #region Lista de objetos

    private void PopulateList(IReadOnlyList<DatabaseObject> objects)
    {
        _updatingChecks = true;
        objectsListView.BeginUpdate();
        try
        {
            _sorter.Reset();
            objectsListView.Items.Clear();
            objectsListView.Items.AddRange(objects.Select(CreateListItem).ToArray());
            selectAllCheckBox.CheckState = CheckState.Unchecked;
        }
        finally
        {
            objectsListView.EndUpdate();
            _updatingChecks = false;
        }

        UpdateListCount();
    }

    private static ListViewItem CreateListItem(DatabaseObject obj)
    {
        var item = new ListViewItem(obj.Owner) { Tag = obj };
        item.SubItems.AddRange(new[]
        {
            obj.Name,
            obj.Type,
            obj.CreatedAt?.ToString(ListViewColumnSorter.DateFormat) ?? "",
            obj.LastDdlAt?.ToString(ListViewColumnSorter.DateFormat) ?? "",
            obj.Status,
            obj.ErrorMessage.ReplaceLineEndings(" | "),
            obj.LastModifiedBy
        });

        if (obj.Status != "VALID")
        {
            item.ForeColor = Color.Firebrick;
        }

        return item;
    }

    private void UpdateListCount() =>
        listCountLabel.Text = $"{objectsListView.Items.Count} itens listados - {objectsListView.CheckedItems.Count} itens selecionados";

    private void ObjectsListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (_updatingChecks)
        {
            return;
        }

        _updatingChecks = true;
        var checkedCount = objectsListView.CheckedItems.Count;
        selectAllCheckBox.CheckState = checkedCount switch
        {
            0 => CheckState.Unchecked,
            _ when checkedCount == objectsListView.Items.Count => CheckState.Checked,
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
        objectsListView.BeginUpdate();
        foreach (ListViewItem item in objectsListView.Items)
        {
            item.Checked = selectAllCheckBox.Checked;
        }
        objectsListView.EndUpdate();
        _updatingChecks = false;
        UpdateListCount();
    }

    private void ObjectsListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        objectsListView.Sort();
    }

    private async void ObjectsListView_DoubleClick(object? sender, EventArgs e)
    {
        if (SelectedObject is { } obj)
        {
            await ExtractSingleAsync(obj.QualifiedName, toClipboard: false);
        }
    }

    private void ObjectsListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C)
        {
            CopyRows(objectsListView.SelectedItems.Cast<ListViewItem>());
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.A)
        {
            foreach (ListViewItem item in objectsListView.Items)
            {
                item.Selected = true;
            }
            e.SuppressKeyPress = true;
        }
    }

    private DatabaseObject? SelectedObject =>
        objectsListView.SelectedItems.Count == 1 ? objectsListView.SelectedItems[0].Tag as DatabaseObject : null;

    private void ObjectContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        objectContextMenu.Items.Clear();

        if (SelectedObject is { } obj)
        {
            objectContextMenu.Items.Add($"Abrir DDL de {obj.QualifiedName} no editor", null,
                async (_, _) => await ExtractSingleAsync(obj.QualifiedName, toClipboard: false));
            objectContextMenu.Items.Add($"Copiar DDL de {obj.QualifiedName}", null,
                async (_, _) => await ExtractSingleAsync(obj.QualifiedName, toClipboard: true));

            if (obj.ErrorMessage.Length > 0)
            {
                objectContextMenu.Items.Add("Mostrar mensagem de erro", null,
                    (_, _) => MessageBox.Show(this, obj.ErrorMessage, $"Erros de {obj.QualifiedName}", MessageBoxButtons.OK, MessageBoxIcon.Information));
            }

            objectContextMenu.Items.Add(new ToolStripSeparator());
        }

        if (objectsListView.SelectedItems.Count > 0)
        {
            objectContextMenu.Items.Add("Copiar linhas selecionadas", null,
                (_, _) => CopyRows(objectsListView.SelectedItems.Cast<ListViewItem>()));
        }

        if (objectsListView.Items.Count > 0)
        {
            objectContextMenu.Items.Add("Copiar toda a lista", null,
                (_, _) => CopyRows(objectsListView.Items.Cast<ListViewItem>()));
            objectContextMenu.Items.Add(new ToolStripSeparator());
        }

        objectContextMenu.Items.Add("Informações do banco de dados", null, async (_, _) => await ShowServerInfoAsync());
        e.Cancel = false;
    }

    private void CopyRows(IEnumerable<ListViewItem> items)
    {
        var rows = items.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var text = new StringBuilder();
        text.AppendJoin('\t', objectsListView.Columns.Cast<ColumnHeader>().Select(c => c.Text)).Append("\r\n");
        foreach (var row in rows)
        {
            text.AppendJoin('\t', row.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)).Append("\r\n");
        }

        _output.CopyToClipboard(text.ToString());
    }

    private void ManualListTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            manualListTextBox.SelectAll();
            e.SuppressKeyPress = true;
        }
    }

    #endregion

    #region Extração

    private void DestinationRadio_CheckedChanged(object? sender, EventArgs e) => UpdateDestinationControls();

    private void UpdateDestinationControls()
    {
        folderTextBox.Enabled = destinationFileRadio.Checked;
        browseFolderButton.Enabled = destinationFileRadio.Checked;
    }

    private void IncludeHeaderCheckBox_CheckedChanged(object? sender, EventArgs e) =>
        _settings.IncludeHeaderOnDdlExtraction = includeHeaderCheckBox.Checked;

    private void InlineErrorsCheckBox_CheckedChanged(object? sender, EventArgs e) =>
        _settings.InlineCompileErrorsOnDdlExtraction = inlineErrorsCheckBox.Checked;

    private void BrowseFolderButton_Click(object? sender, EventArgs e) => SelectFolder();

    private void CancelOperationButton_Click(object? sender, EventArgs e)
    {
        _operationCancellation?.Cancel();
        statusLabel.Text = "Cancelando...";
    }

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

        var options = ExtractionOptions;
        IReadOnlyList<DdlExtractionResult> results = [];
        var completed = await RunAsync("Extraindo DDL", async (progress, ct) =>
            results = await Task.Run(() => _extractor.ExtractManyAsync(names, options, progress, ct), ct));

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
                await _output.SaveToFolderAsync(folder, succeeded);
            }
            else if (destinationClipboardRadio.Checked)
            {
                _output.CopyToClipboard(DdlOutputService.Combine(succeeded));
            }
            else
            {
                var fileName = succeeded.Count == 1 ? succeeded[0].FileName : $"DDL_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                await _output.OpenInEditorAsync(DdlOutputService.Combine(succeeded), fileName);
            }
        }

        var summary = destinationClipboardRadio.Checked
            ? $"DDL de {succeeded.Count} objeto(s) copiada para a área de transferência."
            : $"Extraídos {succeeded.Count} objeto(s)!";

        if (failures.Count > 0)
        {
            if (Ask($"{summary}\n{failures.Count} objeto(s) com erro. Deseja ver as mensagens de erro?"))
            {
                var errors = string.Join("\r\n", failures.Select(f => $"{f.QualifiedName}: {f.Error}"));
                await _output.OpenInEditorAsync(errors, "ErrosExtracaoDDL.txt");
            }
        }
        else if (!destinationEditorRadio.Checked)
        {
            ShowInfo(summary);
        }

        if (folder is not null && succeeded.Count > 0)
        {
            _output.OpenFolder(folder);
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
                ShowError("Nenhum objeto informado na lista.");
                return null;
            }

            var invalid = lines.Where(l => l.Split('.', StringSplitOptions.RemoveEmptyEntries).Length != 2 || l.Any(char.IsWhiteSpace)).ToList();
            if (invalid.Count > 0)
            {
                ShowError($"Lista de objetos inválida. Use OWNER.NOME, um por linha:\n{string.Join("\n", invalid.Take(10))}");
                return null;
            }

            return lines;
        }

        var items = sourceAllRadio.Checked
            ? objectsListView.Items.Cast<ListViewItem>()
            : objectsListView.CheckedItems.Cast<ListViewItem>();

        var names = items.Select(i => ((DatabaseObject)i.Tag!).QualifiedName).Distinct().ToList();
        if (names.Count == 0)
        {
            ShowError(sourceAllRadio.Checked ? "A lista está vazia." : "Nenhum objeto marcado.");
            return null;
        }

        return names;
    }

    private async Task ExtractSingleAsync(string qualifiedName, bool toClipboard)
    {
        var options = ExtractionOptions;
        DdlExtractionResult? result = null;
        await RunAsync($"Extraindo DDL do objeto {qualifiedName}", async (_, ct) =>
            result = await Task.Run(() => _extractor.ExtractAsync(qualifiedName, options, ct), ct));

        if (result is null)
        {
            return;
        }

        if (!result.Succeeded)
        {
            ShowError(result.Error ?? "DDL não encontrada.");
        }
        else if (toClipboard)
        {
            _output.CopyToClipboard(result.Source);
            ShowInfo($"DDL do objeto {qualifiedName} na área de transferência.");
        }
        else
        {
            await _output.OpenInEditorAsync(result.Source, result.FileName);
        }
    }

    private string? SelectFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Pasta onde os arquivos de DDL serão gravados",
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

    #region Conexão e execução

    private bool ShowLogin()
    {
        using var form = _loginFormFactory();
        return form.ShowDialog(this) == DialogResult.OK;
    }

    private async Task RefreshConnectionInfoAsync()
    {
        try
        {
            var serverInfo = await Task.Run(() => _repository.GetServerInfoAsync());
            Text = $"{BaseTitle} - {serverInfo}";

            var selectedOwner = ownerComboBox.Text;
            var owners = await Task.Run(() => _repository.GetOwnersAsync());
            ownerComboBox.BeginUpdate();
            ownerComboBox.Items.Clear();
            ownerComboBox.Items.Add(AllOption);
            ownerComboBox.Items.AddRange(owners.Cast<object>().ToArray());
            ownerComboBox.EndUpdate();
            ownerComboBox.Text = selectedOwner;
        }
        catch (Exception ex)
        {
            Text = $"{BaseTitle} - {_session.Current?.User}@{_session.Current?.DataSource}";
            ShowError($"Problemas ao buscar informações do banco:\n{ex.Message}");
        }
    }

    private async Task ShowServerInfoAsync()
    {
        IReadOnlyList<DatabaseProperty> details = [];
        var completed = await RunAsync("Buscando informações do banco", async (_, ct) =>
            details = await Task.Run(() => _repository.GetDatabaseDetailsAsync(ct), ct));

        if (completed)
        {
            using var form = _databaseInfoFormFactory();
            form.ShowDetails(details);
            form.ShowDialog(this);
        }
    }

    private async Task<bool> RunAsync(string description, Func<IProgress<ProgressInfo>, CancellationToken, Task> operation)
    {
        // O login dispara ConnectionChanged; IsBusy é conferido de novo depois dele (como em DatabaseToolForm).
        if (IsBusy || (!_session.IsConnected && !ShowLogin()) || IsBusy)
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

            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Maximum = Math.Max(1, p.Total);
            progressBar.Value = Math.Min(p.Current, progressBar.Maximum);
            statusLabel.Text = $"{p.Current} de {p.Total} - {p.Message}";
        });

        try
        {
            await operation(progress, cancellation.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Operação cancelada";
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
                BeginInvoke(() => Session_ConnectionChanged(this, EventArgs.Empty));
            }
        }
    }

    private void SetBusy(bool busy, string? description)
    {
        UseWaitCursor = busy;
        filterPanel.Enabled = !busy;
        extractButton.Enabled = !busy;
        sourceGroupBox.Enabled = !busy;
        destinationGroupBox.Enabled = !busy;
        optionsGroupBox.Enabled = !busy;
        fileMenu.Enabled = !busy;
        cancelOperationButton.Enabled = busy;
        progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        progressBar.Value = 0;

        if (busy)
        {
            statusLabel.Text = description;
        }
        else if (statusLabel.Text != "Operação cancelada")
        {
            statusLabel.Text = "Pronto";
        }
    }

    #endregion

    #region Mensagens

    private static string? NullIfAll(string text) =>
        string.IsNullOrWhiteSpace(text) || text.Equals(AllOption, StringComparison.OrdinalIgnoreCase) ? null : text;

    private bool Ask(string message) =>
        MessageBox.Show(this, message, BaseTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void ShowInfo(string message, string? caption = null) =>
        MessageBox.Show(this, message, caption ?? BaseTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void ShowError(string message) =>
        MessageBox.Show(this, message, BaseTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

    #endregion
}
