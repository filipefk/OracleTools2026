using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels.ExtractDdl;

public sealed partial class ExtractDdlViewModel : ToolViewModelBase
{
    public const string AllOption = "Todos";

    private readonly DdlExtractor _extractor;
    private readonly InvalidObjectRecompiler _recompiler;
    private readonly DdlOutputService _output;
    private readonly IAppSettings _settings;
    private bool _loadingSettings;

    public ExtractDdlViewModel(ToolServices services, DdlExtractor extractor, InvalidObjectRecompiler recompiler,
        DdlOutputService output, IAppSettings settings)
        : base(services, "Extract DDL Oracle")
    {
        _extractor = extractor;
        _recompiler = recompiler;
        _output = output;
        _settings = settings;

        Owner = "";
        SelectedType = AllOption;
        NameFilter = "";
        Days = 15;
        ManualList = "";
        OutputFolder = "";
        ReloadSettings();

        Objects.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Objects.Count) or nameof(Objects.CheckedCount))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public ObservableCollection<string> Owners { get; } = [AllOption];

    public IReadOnlyList<string> Types { get; } = [AllOption, .. DatabaseObjectRepository.SupportedTypes];

    [ObservableProperty]
    public partial string Owner { get; set; }

    [ObservableProperty]
    public partial string SelectedType { get; set; }

    [ObservableProperty]
    public partial string NameFilter { get; set; }

    [ObservableProperty]
    public partial double Days { get; set; }

    [ObservableProperty]
    public partial string ManualList { get; set; }

    [ObservableProperty]
    public partial int SourceIndex { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileDestination))]
    public partial int DestinationIndex { get; set; }

    public bool IsFileDestination => Destination == ExtractionDestination.Files;

    [ObservableProperty]
    public partial string OutputFolder { get; set; }

    [ObservableProperty]
    public partial bool IncludeHeader { get; set; }

    [ObservableProperty]
    public partial bool InlineCompileErrors { get; set; }

    public CheckableItemList<DatabaseObjectItem> Objects { get; } = new();

    public string CountText => $"{Objects.Count} itens listados - {Objects.CheckedCount} itens selecionados";

    private ExtractionSource Source => (ExtractionSource)SourceIndex;

    private ExtractionDestination Destination => (ExtractionDestination)DestinationIndex;

    private DdlExtractionOptions ExtractionOptions => new(IncludeHeader, InlineCompileErrors);

    // Relê as opções gravadas (podem ter sido alteradas na página de Parâmetros).
    public void ReloadSettings()
    {
        _loadingSettings = true;
        try
        {
            IncludeHeader = _settings.IncludeHeaderOnDdlExtraction;
            InlineCompileErrors = _settings.InlineCompileErrorsOnDdlExtraction;
            OutputFolder = _settings.DefaultOutputFolder ?? "";
        }
        finally
        {
            _loadingSettings = false;
        }
    }

    partial void OnIncludeHeaderChanged(bool value)
    {
        if (!_loadingSettings)
        {
            _settings.IncludeHeaderOnDdlExtraction = value;
        }
    }

    partial void OnInlineCompileErrorsChanged(bool value)
    {
        if (!_loadingSettings)
        {
            _settings.InlineCompileErrorsOnDdlExtraction = value;
        }
    }

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await LoadOwnersAsync();
        }
    }

    // A conexão é compartilhada com as outras telas. Os objetos listados são do banco anterior e não
    // podem ser extraídos/recompilados no novo.
    protected override async Task OnConnectionChangedAsync()
    {
        Objects.Clear();
        if (Session.IsConnected)
        {
            await LoadOwnersAsync();
        }
    }

    private async Task LoadOwnersAsync()
    {
        try
        {
            var owners = await Task.Run(() => Services.Repository.GetOwnersAsync());
            var typed = Owner;
            Owners.Clear();
            Owners.Add(AllOption);
            foreach (var owner in owners)
            {
                Owners.Add(owner);
            }

            Owner = typed;
        }
        catch (Exception ex)
        {
            ShowError($"Problemas ao buscar informações do banco:\n{ex.Message}");
        }
    }

    private static string? NullIfAll(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Equals(AllOption, StringComparison.OrdinalIgnoreCase) ? null : value.Trim();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task SearchAsync()
    {
        var owner = NullIfAll(Owner);
        var type = NullIfAll(SelectedType);
        var name = NameFilter;
        IReadOnlyList<DatabaseObject> objects = [];
        if (await RunAsync("Listando objetos", async (_, ct) =>
                objects = await Task.Run(() => Services.Repository.SearchAsync(owner, type, name, ct), ct)))
        {
            ShowObjects(objects, "Nenhum objeto encontrado!");
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ListRecentAsync()
    {
        var days = (int)Math.Clamp(double.IsNaN(Days) ? 15 : Days, 1, 3650);
        Owner = "";
        SelectedType = AllOption;
        NameFilter = "";

        IReadOnlyList<DatabaseObject> objects = [];
        if (await RunAsync($"Listando objetos compilados nos últimos {days} dias", async (_, ct) =>
                objects = await Task.Run(() => Services.Repository.GetRecentlyCompiledAsync(days, ct), ct)))
        {
            ShowObjects(objects, $"Nenhum objeto compilado nos últimos {days} dias.");
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task ListInvalidAsync() => ListInvalidObjectsAsync();

    private async Task ListInvalidObjectsAsync()
    {
        IReadOnlyList<DatabaseObject> objects = [];
        if (await RunAsync("Listando objetos inválidos", async (_, ct) =>
                objects = await Task.Run(() => Services.Repository.GetInvalidAsync(ct), ct)))
        {
            ShowObjects(objects, "Nenhum objeto inválido.");
        }
    }

    private void ShowObjects(IReadOnlyList<DatabaseObject> objects, string emptyMessage)
    {
        Objects.Reset(objects.Select(o => new DatabaseObjectItem(o)));
        if (objects.Count == 0)
        {
            ShowInfo(emptyMessage);
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task RecompileInvalidAsync()
    {
        if (!await Host.AskAsync("Recompilar inválidos", "Recompilar todos os objetos inválidos do banco?"))
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

        if (result.AnyRecompiled && await AskAsync("A quantidade de objetos inválidos mudou. Deseja atualizar a lista de objetos inválidos?"))
        {
            await ListInvalidObjectsAsync();
        }

        if (await AskAsync("Deseja visualizar o relatório de compilações?"))
        {
            await OpenInEditorAsync(result.Report, "RelatorioRecompilacao.txt");
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task BrowseFolderAsync() => await SelectFolderAsync();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task OpenDdlAsync(DatabaseObjectItem? item) =>
        item is null ? Task.CompletedTask : ExtractSingleAsync(item.QualifiedName, toClipboard: false);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task CopyDdlAsync(DatabaseObjectItem? item) =>
        item is null ? Task.CompletedTask : ExtractSingleAsync(item.QualifiedName, toClipboard: true);

    [RelayCommand]
    private async Task ShowErrorMessageAsync(DatabaseObjectItem? item)
    {
        if (item is { HasError: true })
        {
            await ShowMessageAsync($"Erros de {item.QualifiedName}", item.Object.ErrorMessage);
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ExtractAsync()
    {
        var names = GetNamesToExtract();
        if (names is null)
        {
            return;
        }

        string? folder = null;
        if (Destination == ExtractionDestination.Files)
        {
            folder = OutputFolder.Trim();
            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                folder = await SelectFolderAsync();
                if (folder is null)
                {
                    return;
                }
            }

            _settings.DefaultOutputFolder = folder;
        }

        var destination = Destination;
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

        try
        {
            if (succeeded.Count > 0)
            {
                if (folder is not null)
                {
                    await _output.SaveToFolderAsync(folder, succeeded);
                }
                else if (destination == ExtractionDestination.Clipboard)
                {
                    Clipboard.SetText(DdlOutputService.Combine(succeeded));
                }
                else
                {
                    var fileName = succeeded.Count == 1 ? succeeded[0].FileName : $"DDL_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                    await _output.OpenInEditorAsync(DdlOutputService.Combine(succeeded), fileName);
                }
            }

            var summary = destination == ExtractionDestination.Clipboard
                ? $"DDL de {succeeded.Count} objeto(s) copiada para a área de transferência."
                : $"Extraídos {succeeded.Count} objeto(s)!";

            if (failures.Count > 0)
            {
                if (await AskAsync($"{summary}\n{failures.Count} objeto(s) com erro. Deseja ver as mensagens de erro?"))
                {
                    var errors = string.Join("\r\n", failures.Select(f => $"{f.QualifiedName}: {f.Error}"));
                    await _output.OpenInEditorAsync(errors, "ErrosExtracaoDDL.txt");
                }
            }
            else if (destination != ExtractionDestination.Editor)
            {
                ShowSuccess(summary);
            }

            if (folder is not null && succeeded.Count > 0)
            {
                _output.OpenFolder(folder);
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private IReadOnlyList<string>? GetNamesToExtract()
    {
        if (Source == ExtractionSource.Manual)
        {
            var lines = ManualList.ReplaceLineEndings("\n").Split('\n')
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

        var items = Source == ExtractionSource.All ? Objects.Items.ToList() : Objects.CheckedItems;
        var names = items.Select(i => i.QualifiedName).Distinct().ToList();
        if (names.Count == 0)
        {
            ShowError(Source == ExtractionSource.All ? "A lista está vazia." : "Nenhum objeto marcado.");
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
            Clipboard.SetText(result.Source);
            ShowInfo($"DDL do objeto {qualifiedName} na área de transferência.");
        }
        else
        {
            await OpenInEditorAsync(result.Source, result.FileName);
        }
    }

    private async Task OpenInEditorAsync(string text, string fileName)
    {
        try
        {
            await _output.OpenInEditorAsync(text, fileName);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task<string?> SelectFolderAsync()
    {
        var initial = Directory.Exists(OutputFolder) ? OutputFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = await Host.PickFolderAsync("Pasta onde os arquivos de DDL serão gravados", initial);
        if (folder is null)
        {
            return null;
        }

        OutputFolder = folder;
        _settings.DefaultOutputFolder = folder;
        return folder;
    }
}
