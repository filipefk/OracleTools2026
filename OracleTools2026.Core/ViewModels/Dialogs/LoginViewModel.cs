using CommunityToolkit.Mvvm.ComponentModel;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels.Dialogs;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IOracleSession _session;
    private readonly IAppSettings _settings;
    private readonly ISavedConnectionStore _savedConnections;

    public LoginViewModel(IOracleSession session, IAppSettings settings, ISavedConnectionStore savedConnections)
    {
        _session = session;
        _settings = settings;
        _savedConnections = savedConnections;

        var current = session.Current;
        User = current?.User ?? settings.LastLoginUser ?? "";
        Password = "";
        DataSource = current?.DataSource ?? settings.LastLoginDataSource ?? "";
        Alias = "";
        ErrorMessage = "";
        LoadDataSources();
        RefreshSavedConnections();
    }

    public ObservableCollection<string> DataSources { get; } = [];

    public ObservableCollection<SavedConnection> SavedConnections { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string User { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string DataSource { get; set; }

    [ObservableProperty]
    public partial bool SaveConnection { get; set; }

    [ObservableProperty]
    public partial bool SavePassword { get; set; }

    [ObservableProperty]
    public partial string Alias { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; private set; }

    public bool HasError => ErrorMessage.Length > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    public partial bool IsConnecting { get; private set; }

    public bool IsIdle => !IsConnecting;

    public string DefaultAlias => $"{User.Trim()}@{DataSource.Trim()}";

    public bool HasTnsNames => !string.IsNullOrWhiteSpace(_settings.TnsNamesPath) && File.Exists(_settings.TnsNamesPath);

    // "Salvar senha" só faz sentido com "Salvar conexão".
    partial void OnSaveConnectionChanged(bool value)
    {
        if (!value)
        {
            SavePassword = false;
        }
    }

    partial void OnUserChanged(string value)
    {
        var upper = value.ToUpperInvariant();
        if (upper != value)
        {
            User = upper;
        }
    }

    public void SetTnsNamesPath(string path)
    {
        _settings.TnsNamesPath = path;
        LoadDataSources();
    }

    // Retorna false se o tnsnames.ora não estiver configurado.
    public bool LoadDataSources()
    {
        if (!HasTnsNames)
        {
            return false;
        }

        var typed = DataSource;
        DataSources.Clear();
        try
        {
            foreach (var alias in TnsNamesParser.ReadAliases(_settings.TnsNamesPath!))
            {
                DataSources.Add(alias);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao ler o tnsnames.ora: {ex.Message}";
        }

        DataSource = typed;
        return DataSources.Count > 0;
    }

    public void ApplySavedConnection(SavedConnection connection)
    {
        User = connection.User;
        Password = connection.Password;
        DataSource = connection.DataSource;
        Alias = connection.Alias;
        SaveConnection = true;
        SavePassword = connection.HasPassword;
    }

    public void RemoveSavedConnection(SavedConnection connection)
    {
        _savedConnections.Remove(connection.Alias);
        RefreshSavedConnections();
    }

    private void RefreshSavedConnections()
    {
        SavedConnections.Clear();
        foreach (var connection in _savedConnections.GetAll())
        {
            SavedConnections.Add(connection);
        }
    }

    // Retorna true quando conectou (o diálogo pode ser fechado).
    public async Task<bool> ConnectAsync()
    {
        ErrorMessage = "";
        var connection = new ConnectionInfo(User.Trim(), Password, DataSource.Trim());
        if (!connection.IsComplete)
        {
            ErrorMessage = "Informe usuário, senha e banco.";
            return false;
        }

        IsConnecting = true;
        try
        {
            await Task.Run(() => _session.ConnectAsync(connection));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsConnecting = false;
        }

        if (SaveConnection)
        {
            var alias = _savedConnections.FindByUserAndDataSource(connection.User, connection.DataSource)?.Alias
                        ?? (Alias.Trim().Length > 0 ? Alias.Trim() : $"{connection.User}@{connection.DataSource}");
            var password = SavePassword ? connection.Password : "";
            _savedConnections.Save(new SavedConnection(alias, connection.User, password, connection.DataSource));
        }

        return true;
    }
}
