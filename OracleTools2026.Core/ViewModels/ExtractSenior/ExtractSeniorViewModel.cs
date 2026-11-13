using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data.Senior;
using OracleTools2026.Models;
using OracleTools2026.Models.Senior;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels.ExtractSenior;

public sealed partial class ExtractSeniorViewModel : ToolViewModelBase
{
    private const int MaxListedItems = 15;

    private readonly SeniorRepository _repository;
    private readonly SeniorDependencyScanner _scanner;
    private readonly SeniorExportWriter _writer;
    private readonly DdlOutputService _output;
    private readonly IAppSettings _settings;
    private ConnectionInfo? _ownersConnection;
    // O mesmo nome pode aparecer mais de uma vez (ex.: um serviço com o mesmo SRVNAM em dois provedores).
    private ILookup<SeniorItemKey, SeniorItem> _itemsByKey = Array.Empty<SeniorItem>().ToLookup(i => i.Key);

    public ExtractSeniorViewModel(ToolServices services, SeniorRepository repository, SeniorDependencyScanner scanner,
        SeniorExportWriter writer, DdlOutputService output, IAppSettings settings)
        : base(services, "Exportar artefatos Senior")
    {
        _repository = repository;
        _scanner = scanner;
        _writer = writer;
        _output = output;
        _settings = settings;

        OutputFolder = settings.DefaultOutputFolder ?? "";
        IncludeWebServices = true;
        IncludeCbds = true;
        IncludeRuleLinks = true;
        IncludeTableDdl = true;

        Items.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Items.Count) or nameof(Items.CheckedCount))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public ObservableCollection<string> Owners { get; } = [];

    [ObservableProperty]
    public partial string? SelectedOwner { get; set; }

    [ObservableProperty]
    public partial bool IncludeWebServices { get; set; }

    [ObservableProperty]
    public partial bool IncludeCbds { get; set; }

    [ObservableProperty]
    public partial bool IncludeRuleLinks { get; set; }

    [ObservableProperty]
    public partial bool IncludeTableDdl { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RootFolderText))]
    public partial string OutputFolder { get; set; }

    // Itens listados (ou exportados) com a situação de cada um na última exportação.
    public CheckableItemList<SeniorItem> Items { get; } = new();

    public string CountText
    {
        get
        {
            if (Items.Count == 0)
            {
                return "Clique em Listar para ver os itens antes de exportar.";
            }

            var kinds = Items.Items
                .GroupBy(i => i.Kind)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Count()} {KindCountText(g.Key)}");
            var text = $"{Items.Count} itens ({string.Join(", ", kinds)}) - {Items.CheckedCount} marcados";

            var written = Items.Items.Count(i => i.Status == SeniorItemStatus.Written);
            var unchanged = Items.Items.Count(i => i.Status == SeniorItemStatus.Unchanged);
            var failed = Items.Items.Count(i => i.Status == SeniorItemStatus.Error);
            var notFound = Items.Items.Count(i => i.Status == SeniorItemStatus.NotFound);
            if (written + unchanged + failed + notFound > 0)
            {
                text += $" - última exportação: {written} gravado(s), {unchanged} sem alteração, {failed} com erro" +
                        (notFound > 0 ? $", {notFound} não encontrado(s)" : "");
            }

            return text;
        }
    }

    public string RootFolderText => OutputFolder.Trim().Length == 0
        ? "Os arquivos são gravados na subpasta senior da pasta escolhida."
        : $"Os arquivos são gravados em {Path.Combine(OutputFolder.Trim(), "senior")}.";

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await LoadOwnersAsync();
        }
    }

    // Os schemas listados são do banco anterior. São recarregados ao conectar por esta tela, ao abrir a
    // lista ou ao exportar, para não concorrer com a operação que pediu o login.
    protected override Task OnConnectionChangedAsync()
    {
        if (!ReferenceEquals(_ownersConnection, Session.Current))
        {
            Owners.Clear();
            SelectedOwner = null;
            _ownersConnection = null;
            ClearItems();
        }

        return Task.CompletedTask;
    }

    // A lista vale para o schema e os tipos com que foi lida.
    partial void OnSelectedOwnerChanged(string? value) => ClearItems();

    partial void OnIncludeWebServicesChanged(bool value) => ClearItems();

    partial void OnIncludeCbdsChanged(bool value) => ClearItems();

    partial void OnIncludeRuleLinksChanged(bool value) => ClearItems();

    partial void OnIncludeTableDdlChanged(bool value) => ClearItems();

    protected override async Task OnConnectRequestedAsync()
    {
        if (await ShowLoginAsync())
        {
            await LoadOwnersAsync();
        }
    }

    // Chamado ao abrir a lista de schemas.
    public async Task EnsureOwnersLoadedAsync()
    {
        if (!OwnersLoaded && Session.IsConnected && !IsBusy)
        {
            await LoadOwnersAsync();
        }
    }

    private bool OwnersLoaded => _ownersConnection is not null && ReferenceEquals(_ownersConnection, Session.Current);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task RefreshOwnersAsync() => LoadOwnersAsync();

    private async Task LoadOwnersAsync()
    {
        IReadOnlyList<string> owners = [];
        if (!await RunAsync("Procurando schemas com as tabelas do Senior", async (_, ct) =>
                owners = await Task.Run(() => _repository.GetOwnersAsync(ct), ct)))
        {
            return;
        }

        var previous = SelectedOwner;
        _ownersConnection = Session.Current;
        Owners.Clear();
        foreach (var owner in owners)
        {
            Owners.Add(owner);
        }

        // Padrão: o schema selecionado antes ou o usuário conectado.
        var user = Session.Current?.User.Trim().ToUpperInvariant();
        SelectedOwner = owners.FirstOrDefault(o => o == previous) ?? owners.FirstOrDefault(o => o == user) ?? owners.FirstOrDefault();

        if (owners.Count == 0)
        {
            ShowWarning("Nenhum schema com a tabela R960SRV acessível para o usuário conectado.");
        }
        else
        {
            StatusText = $"{owners.Count} schema(s) do Senior encontrados às {Now()}";
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task BrowseFolderAsync() => await SelectFolderAsync();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private void OpenFolder()
    {
        var folder = OutputFolder.Trim();
        var root = Path.Combine(folder, "senior");
        if (folder.Length == 0 || !Directory.Exists(folder))
        {
            ShowError("A pasta de destino não existe.");
            return;
        }

        try
        {
            _output.OpenFolder(Directory.Exists(root) ? root : folder);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ListAsync()
    {
        if (await PrepareAsync() is not { } owner)
        {
            return;
        }

        var options = CreateOptions(owner, OutputFolder.Trim(), null);
        if (!options.HasAnyType)
        {
            ShowError("Marque pelo menos um tipo de artefato.");
            return;
        }

        IReadOnlyList<SeniorItemSummary> items = [];
        if (!await RunAsync($"Listando artefatos do schema {owner}", async (progress, ct) =>
                items = await Task.Run(() => _repository.ListItemsAsync(options, progress, ct), ct)))
        {
            return;
        }

        LoadItems(items);
        StatusText = $"{items.Count} item(ns) listados às {Now()}";
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ExtractAsync()
    {
        if (await PrepareAsync() is not { } owner)
        {
            return;
        }

        var folder = OutputFolder.Trim();
        if (folder.Length == 0 || !Directory.Exists(folder))
        {
            folder = await SelectFolderAsync();
            if (folder is null)
            {
                return;
            }
        }

        _settings.DefaultOutputFolder = folder;
        if (!CreateOptions(owner, folder, null).HasAnyType)
        {
            ShowError("Marque pelo menos um tipo de artefato.");
            return;
        }

        // Com a lista carregada, grava só os itens marcados. Com todos marcados a exportação é completa
        // (inclusive itens criados depois da listagem), para os órfãos poderem ser apagados.
        var listed = Items.Count > 0;
        IReadOnlySet<SeniorItemKey>? selection = null;
        if (listed)
        {
            var checkedItems = Items.CheckedItems;
            if (checkedItems.Count == 0)
            {
                ShowError("Marque pelo menos um item da lista.");
                return;
            }

            if (checkedItems.Count < Items.Count)
            {
                selection = checkedItems.Select(i => i.Key).ToHashSet();
            }

            foreach (var item in Items.Items)
            {
                item.Status = item.IsChecked ? SeniorItemStatus.Pending : SeniorItemStatus.NotChecked;
                item.Message = "";
            }

            OnPropertyChanged(nameof(CountText));
        }

        var options = CreateOptions(owner, folder, selection);
        var itemProgress = new Progress<SeniorItemProgress>(ApplyOutcome);
        SeniorExportData? data = null;
        SeniorDependencyScan? dependencies = null;
        SeniorWriteResult? written = null;
        IReadOnlyList<string> orphans = [];
        var completed = await RunAsync($"Exportando artefatos do schema {owner}", async (progress, ct) =>
        {
            var exported = await Task.Run(() => _repository.ExportAsync(options, progress, ct), ct);

            // As dependências usam todos os serviços lidos, para o arquivo não ficar incompleto na
            // exportação parcial.
            var scan = options.IncludeWebServices ? _scanner.Scan(exported.Services) : null;
            var toWrite = selection is null ? exported : exported.Filter(selection);
            ShowExportedItems(toWrite.Items(options), replace: !listed);

            var result = await Task.Run(() => _writer.WriteAsync(options, toWrite, scan, progress, itemProgress, ct), ct);
            orphans = options.IsPartial ? [] : _writer.FindOrphans(options, result.Files);
            (data, dependencies, written) = (toWrite, scan, result);
        });

        if (!completed || data is null || written is null)
        {
            foreach (var item in Items.Items.Where(i => i.Status == SeniorItemStatus.Pending))
            {
                item.Status = SeniorItemStatus.None;
            }

            OnPropertyChanged(nameof(CountText));
            return;
        }

        foreach (var outcome in written.ItemOutcomes)
        {
            ApplyOutcome(outcome);
        }

        foreach (var item in Items.Items.Where(i => i.Status == SeniorItemStatus.Pending))
        {
            item.Status = SeniorItemStatus.NotFound;
            item.Message = "Não existe mais no banco ou não foi lido nesta exportação.";
        }

        OnPropertyChanged(nameof(CountText));

        var deleted = 0;
        try
        {
            if (orphans.Count > 0 && await AskAsync(
                    $"{orphans.Count} arquivo(s) em {options.RootFolder} não vieram desta exportação " +
                    $"(removidos do banco?):\n\n{ListItems(orphans.Select(o => Path.GetRelativePath(options.RootFolder, o)).ToList())}\n\n" +
                    "Deseja apagá-los?"))
            {
                deleted = _writer.DeleteOrphans(options, orphans);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erro ao apagar arquivos órfãos: {ex.Message}");
            return;
        }

        var summary = new SeniorExportSummary(
            data.Services.Count,
            data.Services.Sum(s => s.Ports.Count),
            data.Tables.Count,
            data.StandardTableFields.Select(f => f.TableName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            data.Lists.Count,
            data.Views.Count,
            data.RuleLinks.Count,
            data.TableDdls.Count(d => d.Ddl is not null),
            written.Written,
            deleted,
            written.Errors,
            dependencies?.SecretWarnings ?? []);

        ShowSummary(options, summary, written.Unchanged);
        StatusText = $"Exportação concluída às {Now()}";
    }

    // Conexão e schema escolhido, comuns a Listar e Exportar.
    private async Task<string?> PrepareAsync()
    {
        if (!await EnsureConnectedAsync())
        {
            return null;
        }

        if (!OwnersLoaded)
        {
            await LoadOwnersAsync();
        }

        if (SelectedOwner is not { } owner)
        {
            ShowError("Escolha o schema do Senior.");
            return null;
        }

        return owner;
    }

    private SeniorExportOptions CreateOptions(string owner, string folder, IReadOnlySet<SeniorItemKey>? selection) =>
        new(owner, folder, IncludeWebServices, IncludeCbds, IncludeRuleLinks, IncludeTableDdl, selection);

    // Mais recentes primeiro; sem data, no fim.
    private void LoadItems(IEnumerable<SeniorItemSummary> summaries)
    {
        var items = summaries
            .OrderByDescending(s => s.LastChange ?? DateTime.MinValue)
            .ThenBy(s => s.Key.Kind)
            .ThenBy(s => s.Key.Name, StringComparer.Ordinal)
            .Select(s => new SeniorItem(s))
            .ToList();
        SetItems(items);
    }

    // Exportação sem lista: mostra os itens que serão gravados. Com a lista completa: acrescenta os itens
    // criados no banco depois da listagem.
    private void ShowExportedItems(IReadOnlyList<SeniorItemSummary> summaries, bool replace)
    {
        if (replace)
        {
            LoadItems(summaries);
            foreach (var item in Items.Items)
            {
                item.Status = SeniorItemStatus.Pending;
            }
        }
        else
        {
            var added = summaries
                .Where(s => !_itemsByKey.Contains(s.Key))
                .Select(s => new SeniorItem(s) { Status = SeniorItemStatus.Pending })
                .ToList();
            if (added.Count > 0)
            {
                SetItems([.. Items.Items, .. added]);
            }
        }

        OnPropertyChanged(nameof(CountText));
    }

    private void SetItems(IReadOnlyList<SeniorItem> items)
    {
        _itemsByKey = items.ToLookup(i => i.Key);
        foreach (var group in _itemsByKey.Where(g => g.Count() > 1))
        {
            foreach (var item in group.Where(i => i.Message.Length == 0))
            {
                item.Message = $"Nome repetido no banco ({group.Count()} vezes): gera os mesmos arquivos.";
            }
        }

        Items.Reset(items);
    }

    private void ClearItems()
    {
        if (Items.Count > 0)
        {
            SetItems([]);
        }
    }

    private void ApplyOutcome(SeniorItemProgress progress)
    {
        foreach (var item in _itemsByKey[progress.Key])
        {
            item.SetOutcome(progress);
        }

        OnPropertyChanged(nameof(CountText));
    }

    private static string KindCountText(SeniorItemKind kind) => kind switch
    {
        SeniorItemKind.WebService => "webservice(s)",
        SeniorItemKind.Table => "tabela(s)",
        SeniorItemKind.StandardTableFields => "tabela(s) padrão",
        SeniorItemKind.List => "lista(s)",
        SeniorItemKind.View => "visão(ões)",
        SeniorItemKind.RuleLink => "regra(s)",
        _ => kind.ToString()
    };

    private void ShowSummary(SeniorExportOptions options, SeniorExportSummary summary, int unchanged)
    {
        var counts = new List<string>();
        if (options.IncludeWebServices)
        {
            counts.Add($"{summary.Services} webservice(s) com {summary.Ports} porta(s)");
        }

        if (options.IncludeCbds)
        {
            counts.Add($"{summary.Tables} tabela(s), campos em {summary.StandardTables} tabela(s) padrão, " +
                       $"{summary.Lists} lista(s), {summary.Views} visão(ões)");
        }

        if (options.IncludeRuleLinks)
        {
            counts.Add($"{summary.RuleLinks} regra(s) com vínculos");
        }

        if (options.IncludeTableDdl)
        {
            counts.Add($"{summary.TableDdls} DDL(s) de tabela");
        }

        var message = (options.IsPartial ? $"Exportação parcial ({options.Selection!.Count} item(ns) marcados; arquivos órfãos não verificados). " : "") +
                      $"Exportados {string.Join("; ", counts)}. " +
                      $"{summary.FilesWritten} arquivo(s) gravado(s), {unchanged} sem alteração" +
                      (summary.FilesDeleted > 0 ? $", {summary.FilesDeleted} apagado(s)." : ".");

        if (summary.Errors.Count > 0)
        {
            message += $"\n\n{summary.Errors.Count} erro(s):\n{ListItems(summary.Errors)}";
        }

        if (summary.SecretWarnings.Count > 0)
        {
            message += $"\n\nPossíveis segredos no fonte das regras (revise antes do commit):\n{ListItems(summary.SecretWarnings)}";
        }

        if (summary.Errors.Count > 0 || summary.SecretWarnings.Count > 0)
        {
            ShowWarning(message);
        }
        else
        {
            ShowSuccess(message);
        }
    }

    private static string ListItems(IReadOnlyList<string> items)
    {
        var text = string.Join("\n", items.Take(MaxListedItems).Select(i => "• " + i));
        return items.Count > MaxListedItems ? $"{text}\n... e mais {items.Count - MaxListedItems}" : text;
    }

    private async Task<string?> SelectFolderAsync()
    {
        var initial = Directory.Exists(OutputFolder) ? OutputFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = await Host.PickFolderAsync("Pasta do repositório (os arquivos vão para a subpasta senior)", initial);
        if (folder is null)
        {
            return null;
        }

        OutputFolder = folder;
        _settings.DefaultOutputFolder = folder;
        return folder;
    }
}
