using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.ViewModels.Sessions;

public sealed partial class SessionsViewModel : ToolViewModelBase
{
    private readonly SessionRepository _repository;
    private readonly RealNameDirectory _realNames;

    public SessionsViewModel(ToolServices services, SessionRepository repository, RealNameDirectory realNames)
        : base(services, "Sessões")
    {
        _repository = repository;
        _realNames = realNames;
        Sessions.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Sessions.Count) or nameof(Sessions.CheckedCount))
            {
                OnPropertyChanged(nameof(CountText));
            }
        };
    }

    public CheckableItemList<SessionItem> Sessions { get; } = new();

    public string CountText => $"{Sessions.Count} sessões listadas - {Sessions.CheckedCount} marcadas";

    // Como no original, considera as sessões selecionadas e as marcadas.
    public IReadOnlyList<SessionItem> KillTargets => Sessions.Selected.Concat(Sessions.CheckedItems).Distinct().ToList();

    protected override async Task OnActivatedAsync()
    {
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    // A lista do banco anterior é descartada: matar sessões dela usaria SID/SERIAL# de outro banco.
    protected override async Task OnConnectionChangedAsync()
    {
        Sessions.Clear();
        if (Session.IsConnected)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task RefreshAsync()
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

        Sessions.Reset(sessions.Select(s => new SessionItem(s, names)));
        StatusText = namesError is null
            ? $"{sessions.Count} sessões listadas às {Now()}"
            : $"{sessions.Count} sessões listadas - não foi possível buscar os nomes reais: {namesError}";
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task KillAsync()
    {
        var targets = KillTargets.Select(i => i.Session).ToList();
        if (targets.Count == 0)
        {
            ShowInfo("Marque ou selecione as sessões que deseja matar.");
            return;
        }

        var description = targets.Count == 1
            ? $"a sessão {targets[0].Sid},{targets[0].SerialNumber} do usuário {targets[0].UserName} ({targets[0].OsUser} em {targets[0].Machine})"
            : $"as {targets.Count} sessões selecionadas";
        if (!await AskAsync($"Confirma matar {description}?"))
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

        if (await AskAsync($"{message}\n\nDeseja atualizar a lista de sessões?"))
        {
            await RefreshAsync();
        }
    }
}
