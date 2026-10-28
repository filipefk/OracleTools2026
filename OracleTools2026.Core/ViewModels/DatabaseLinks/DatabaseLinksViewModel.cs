using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;

namespace OracleTools2026.ViewModels.DatabaseLinks;

public sealed partial class DatabaseLinksViewModel : ToolViewModelBase
{
    private readonly DatabaseLinkRepository _repository;

    public DatabaseLinksViewModel(ToolServices services, DatabaseLinkRepository repository)
        : base(services, "DBLINKs")
    {
        _repository = repository;
        Links.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Links.Count))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public ItemList<DatabaseLinkItem> Links { get; } = new();

    public string CountText => $"{Links.Count} DBLINKs listados";

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    protected override async Task OnConnectionChangedAsync()
    {
        Links.Clear();
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task RefreshAsync()
    {
        IReadOnlyList<DatabaseLink> links = [];
        var completed = await RunAsync("Listando DBLINKs", async (_, ct) =>
            links = await Task.Run(() => _repository.GetLinksAsync(ct), ct));

        if (!completed)
        {
            return;
        }

        Links.Reset(links.Select(l => new DatabaseLinkItem(l)));
        StatusText = $"{links.Count} DBLINKs listados às {Now()}";
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task TestAllAsync() => TestAsync(Links.Items.ToList());

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private Task TestSelectedAsync() => TestAsync(Links.Selected);

    private async Task TestAsync(IReadOnlyList<DatabaseLinkItem> items)
    {
        if (items.Count == 0)
        {
            ShowInfo(Links.Count == 0 ? "A lista está vazia." : "Selecione os DBLINKs que deseja testar.");
            return;
        }

        var itemsByLink = items.ToDictionary(i => i.Link);
        foreach (var item in items)
        {
            item.SetPending();
        }

        // Atualiza cada linha assim que o seu teste termina.
        var testedLink = new Progress<DatabaseLinkTestResult>(result =>
        {
            if (itemsByLink.TryGetValue(result.Link, out var item))
            {
                item.SetResult(result);
            }
        });

        IReadOnlyList<DatabaseLinkTestResult> results = [];
        var links = itemsByLink.Keys.ToList();
        var completed = await RunAsync("Testando DBLINKs", async (progress, ct) =>
            results = await Task.Run(() => _repository.TestAsync(links, progress, testedLink, ct), ct));

        foreach (var item in items)
        {
            item.ClearPending();
        }

        if (completed)
        {
            var failures = results.Count(r => !r.Succeeded);
            StatusText = $"{results.Count} DBLINKs testados às {Now()}: {results.Count - failures} OK, {failures} com erro";
        }
    }
}
