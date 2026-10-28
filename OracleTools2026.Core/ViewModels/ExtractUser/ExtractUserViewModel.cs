using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels.ExtractUser;

public sealed partial class ExtractUserViewModel : ToolViewModelBase
{
    public const string AllOption = "Todos";

    private readonly DatabaseUserRepository _repository;
    private readonly UserScriptGenerator _generator;
    private readonly DdlOutputService _output;
    private readonly IAppSettings _settings;

    public ExtractUserViewModel(ToolServices services, DatabaseUserRepository repository, UserScriptGenerator generator,
        DdlOutputService output, IAppSettings settings)
        : base(services, "Extrair script de usuários")
    {
        _repository = repository;
        _generator = generator;
        _output = output;
        _settings = settings;

        Profiles = [AllOption];
        SelectedProfile = AllOption;
        NameFilter = "";
        CustomFilter = settings.UserSearchFilter ?? "";
        ManualList = "";
        OutputFolder = settings.DefaultOutputFolder ?? "";

        Users.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Users.Count) or nameof(Users.CheckedCount))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public IReadOnlyList<string> OwnerFilterOptions { get; } = [AllOption, "Que seja owner", "Que não seja owner"];

    [ObservableProperty]
    public partial int OwnerFilterIndex { get; set; }

    public ObservableCollection<string> Profiles { get; }

    [ObservableProperty]
    public partial string? SelectedProfile { get; set; }

    [ObservableProperty]
    public partial string NameFilter { get; set; }

    [ObservableProperty]
    public partial string CustomFilter { get; set; }

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

    public CheckableItemList<DatabaseUserItem> Users { get; } = new();

    public string CountText => $"{Users.Count} usuários listados - {Users.CheckedCount} usuários selecionados";

    private ExtractionSource Source => (ExtractionSource)SourceIndex;

    private ExtractionDestination Destination => (ExtractionDestination)DestinationIndex;

    private bool ProfilesLoaded => Profiles.Count > 1;

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await LoadProfilesAsync();
        }
    }

    // Os usuários listados e os profiles são do banco anterior. Os profiles são recarregados na próxima
    // listagem (ou ao abrir a lista de profiles), para não concorrer com a operação que pediu o login.
    protected override Task OnConnectionChangedAsync()
    {
        Users.Clear();
        ResetProfiles([]);
        return Task.CompletedTask;
    }

    // Chamado ao abrir a lista de profiles, depois de trocar a conexão.
    public async Task EnsureProfilesLoadedAsync()
    {
        if (!ProfilesLoaded && Session.IsConnected && !IsBusy)
        {
            await LoadProfilesAsync();
        }
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
        var selected = SelectedProfile;
        Profiles.Clear();
        Profiles.Add(AllOption);
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }

        SelectedProfile = selected is not null && profiles.Contains(selected) ? selected : AllOption;
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task SearchAsync()
    {
        var customFilter = CustomFilter.Trim();
        _settings.UserSearchFilter = customFilter;

        var criteria = new UserSearchCriteria(
            (UserOwnerFilter)Math.Max(0, OwnerFilterIndex),
            SelectedProfile is { } profile && profile != AllOption ? profile : null,
            NameFilter.Trim(),
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

        Users.Reset(users.Select(u => new DatabaseUserItem(u)));
        StatusText = $"{users.Count} usuários listados às {Now()}";
    }

    [RelayCommand]
    private void OpenUserDetails(DatabaseUserItem? item) =>
        Services.Navigator.Open(ToolCommands.UserDetails, item?.Name ?? Users.SelectedItem?.Name);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task BrowseFolderAsync() => await SelectFolderAsync();

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task ViewScriptAsync(DatabaseUserItem? item) =>
        item is null ? Task.CompletedTask : ExtractSingleAsync(item.Name, toClipboard: false);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task CopyScriptAsync(DatabaseUserItem? item) =>
        item is null ? Task.CompletedTask : ExtractSingleAsync(item.Name, toClipboard: true);

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
        IReadOnlyList<UserScriptResult> results = [];
        var completed = await RunAsync("Extraindo scripts de usuários", async (progress, ct) =>
            results = await Task.Run(() => _generator.GenerateManyAsync(names, progress, ct), ct));

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
                    await _output.SaveUserScriptsAsync(folder, succeeded);
                }
                else if (destination == ExtractionDestination.Clipboard)
                {
                    Clipboard.SetText(DdlOutputService.Combine(succeeded));
                }
                else
                {
                    var fileName = succeeded.Count == 1 ? succeeded[0].FileName : $"Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                    await _output.OpenInEditorAsync(DdlOutputService.Combine(succeeded), fileName);
                }
            }

            var summary = destination == ExtractionDestination.Clipboard
                ? $"Script de {succeeded.Count} usuário(s) copiado para a área de transferência."
                : $"Extraídos {succeeded.Count} usuário(s)!";

            if (failures.Count > 0)
            {
                if (await AskAsync($"{summary}\n{failures.Count} usuário(s) com erro. Deseja ver as mensagens de erro?"))
                {
                    var errors = string.Join("\r\n", failures.Select(f => f.Error));
                    await _output.OpenInEditorAsync(errors, "ErrosExtracaoUsuarios.txt");
                }
            }
            else if (destination != ExtractionDestination.Editor)
            {
                ShowSuccess(summary);
            }

            if (folder is not null && succeeded.Count > 0)
            {
                _output.OpenFolder(Path.Combine(folder, "USUARIOS"));
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

        var items = Source == ExtractionSource.All ? Users.Items.ToList() : Users.CheckedItems;
        var names = items.Select(i => i.Name).Distinct().ToList();
        if (names.Count == 0)
        {
            ShowError(Source == ExtractionSource.All ? "A lista está vazia." : "Nenhum usuário marcado.");
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

        try
        {
            if (!result.Succeeded)
            {
                ShowError(result.Error ?? "Script não gerado.");
            }
            else if (toClipboard)
            {
                Clipboard.SetText(result.Script);
                ShowInfo($"Script do usuário {userName} na área de transferência.");
            }
            else
            {
                await _output.OpenInEditorAsync(result.Script, result.FileName);
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task<string?> SelectFolderAsync()
    {
        var initial = Directory.Exists(OutputFolder) ? OutputFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = await Host.PickFolderAsync("Pasta onde os scripts serão gravados (na subpasta USUARIOS)", initial);
        if (folder is null)
        {
            return null;
        }

        OutputFolder = folder;
        _settings.DefaultOutputFolder = folder;
        return folder;
    }
}
