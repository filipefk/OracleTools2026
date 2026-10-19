using OracleTools2026.Data;
using OracleTools2026.Models;

namespace OracleTools2026.UI;

public partial class DatabaseLinksForm : DatabaseToolForm
{
    private const int TestColumnIndex = 3;

    private readonly DatabaseLinkRepository _repository;
    private readonly ListViewColumnSorter _sorter = new();

    public DatabaseLinksForm(DatabaseToolDependencies dependencies, DatabaseLinkRepository repository)
        : base(dependencies, "DBLINKs")
    {
        _repository = repository;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
        linksListView.ListViewItemSorter = _sorter;
    }

    private async void DatabaseLinksForm_Load(object? sender, EventArgs e)
    {
        UpdateListCount();
        if (Session.IsConnected)
        {
            await RefreshLinksAsync();
        }
    }

    protected override async void OnConnectionChanged()
    {
        PopulateList([]);
        if (Session.IsConnected)
        {
            await RefreshLinksAsync();
        }
    }

    protected override void OnBusyChanged(bool busy)
    {
        refreshButton.Enabled = !busy;
        testAllButton.Enabled = !busy;
        testSelectedButton.Enabled = !busy;
        fileMenu.Enabled = !busy;
        cancelButton.Enabled = busy;
    }

    #region Menu e botões

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    private async void RefreshButton_Click(object? sender, EventArgs e) => await RefreshLinksAsync();

    private async void TestAllButton_Click(object? sender, EventArgs e) =>
        await TestLinksAsync(linksListView.Items.Cast<ListViewItem>().ToList());

    private async void TestSelectedButton_Click(object? sender, EventArgs e) =>
        await TestLinksAsync(linksListView.SelectedItems.Cast<ListViewItem>().ToList());

    private void CancelButton_Click(object? sender, EventArgs e) => CancelOperation();

    #endregion

    #region Lista de DBLINKs

    private async Task RefreshLinksAsync()
    {
        IReadOnlyList<DatabaseLink> links = [];
        var completed = await RunAsync("Listando DBLINKs", async (_, ct) =>
            links = await Task.Run(() => _repository.GetLinksAsync(ct), ct));

        if (!completed)
        {
            return;
        }

        PopulateList(links);
        SetStatus($"{links.Count} DBLINKs listados às {DateTime.Now:HH:mm:ss}");
    }

    private void PopulateList(IReadOnlyList<DatabaseLink> links)
    {
        linksListView.BeginUpdate();
        try
        {
            _sorter.Reset();
            linksListView.Items.Clear();
            linksListView.Items.AddRange(links.Select(CreateListItem).ToArray());
        }
        finally
        {
            linksListView.EndUpdate();
        }

        linksListView.AutoResizeColumnsToContent();
        UpdateListCount();
    }

    private static ListViewItem CreateListItem(DatabaseLink link)
    {
        var item = new ListViewItem(ListViewExtensions.FormatDate(link.CreatedAt)) { Tag = link };
        item.SubItems.AddRange(new[] { link.Owner, link.Name, "", link.UserName, link.Host });
        return item;
    }

    private void UpdateListCount() =>
        listCountLabel.Text = $"{linksListView.Items.Count} DBLINKs listados";

    private void LinksListView_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        _sorter.Toggle(e.Column);
        linksListView.Sort();
    }

    private void LinksListView_KeyDown(object? sender, KeyEventArgs e) => linksListView.HandleCopyShortcuts(e);

    private void LinkContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        linkContextMenu.Items.Clear();

        if (linksListView.SelectedItems.Count > 0)
        {
            linkContextMenu.Items.Add("Testar DBLINKs selecionados", null,
                async (_, _) => await TestLinksAsync(linksListView.SelectedItems.Cast<ListViewItem>().ToList()));
            linkContextMenu.Items.Add(new ToolStripSeparator());
            linkContextMenu.Items.Add("Copiar linhas selecionadas", null, (_, _) => linksListView.CopySelectedToClipboard());
        }

        if (linksListView.Items.Count > 0)
        {
            linkContextMenu.Items.Add("Copiar toda a lista", null, (_, _) => linksListView.CopyAllToClipboard());
            linkContextMenu.Items.Add(new ToolStripSeparator());
        }

        linkContextMenu.Items.Add("Informações do banco de dados", null, async (_, _) => await ShowServerInfoAsync());
        e.Cancel = IsBusy;
    }

    #endregion

    #region Teste de conectividade

    private async Task TestLinksAsync(IReadOnlyList<ListViewItem> items)
    {
        if (items.Count == 0)
        {
            ShowInfo(linksListView.Items.Count == 0 ? "A lista está vazia." : "Selecione os DBLINKs que deseja testar.");
            return;
        }

        var itemsByLink = items.ToDictionary(i => (DatabaseLink)i.Tag!);
        foreach (var item in items)
        {
            SetTestResult(item, "Aguardando teste...", SystemColors.GrayText);
        }

        // Atualiza cada linha assim que o seu teste termina.
        var testedLink = new Progress<DatabaseLinkTestResult>(result =>
        {
            if (itemsByLink.TryGetValue(result.Link, out var item))
            {
                if (result.Succeeded)
                {
                    SetTestResult(item, "TESTE OK", Color.DarkGreen);
                }
                else
                {
                    SetTestResult(item, $"ERRO: {result.Error?.ReplaceLineEndings(" ")}", Color.Red);
                }
            }
        });

        IReadOnlyList<DatabaseLinkTestResult> results = [];
        var links = itemsByLink.Keys.ToList();
        var completed = await RunAsync("Testando DBLINKs", async (progress, ct) =>
            results = await Task.Run(() => _repository.TestAsync(links, progress, testedLink, ct), ct));

        foreach (var item in items.Where(i => i.SubItems[TestColumnIndex].Text == "Aguardando teste..."))
        {
            SetTestResult(item, "", linksListView.ForeColor);
        }

        linksListView.AutoResizeColumnsToContent();

        if (completed)
        {
            var failures = results.Count(r => !r.Succeeded);
            SetStatus($"{results.Count} DBLINKs testados às {DateTime.Now:HH:mm:ss}: {results.Count - failures} OK, {failures} com erro");
        }
    }

    private static void SetTestResult(ListViewItem item, string text, Color color)
    {
        item.SubItems[TestColumnIndex].Text = text;
        item.ForeColor = color;
    }

    #endregion
}
