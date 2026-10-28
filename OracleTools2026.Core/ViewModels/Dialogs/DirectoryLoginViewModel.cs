using CommunityToolkit.Mvvm.ComponentModel;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace OracleTools2026.ViewModels.Dialogs;

public sealed partial class DirectoryLoginViewModel : ObservableObject
{
    private const string DefaultPort = "389";

    private readonly ActiveDirectoryService _directory;
    private readonly ISavedDirectoryConnectionStore _savedConnections;

    public DirectoryLoginViewModel(ActiveDirectoryService directory, ISavedDirectoryConnectionStore savedConnections)
    {
        _directory = directory;
        _savedConnections = savedConnections;

        Domain = "";
        Port = DefaultPort;
        User = "";
        Password = "";
        Alias = "";
        ErrorMessage = "";
        RefreshSavedConnections();
    }

    public ObservableCollection<SavedDirectoryConnection> SavedConnections { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string Domain { get; set; }

    [ObservableProperty]
    public partial string Port { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string User { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

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

    public DirectoryConnection? Connection { get; private set; }

    public string DefaultAlias => CurrentConnection.ToString();

    private DirectoryConnection CurrentConnection => new(Domain.Trim(), Port.Trim(), User.Trim(), Password);

    // Usa a conexão atual da tela; sem ela, a primeira conexão salva; sem nenhuma, o domínio do computador.
    public void Initialize(DirectoryConnection? current)
    {
        if (current is not null)
        {
            ApplyConnection(current);
        }
        else if (_savedConnections.GetAll() is [var first, ..])
        {
            ApplySavedConnection(first);
        }
        else
        {
            Domain = GetComputerDomain();
            Port = DefaultPort;
        }
    }

    partial void OnSaveConnectionChanged(bool value)
    {
        if (!value)
        {
            SavePassword = false;
        }
    }

    public void ApplySavedConnection(SavedDirectoryConnection saved)
    {
        ApplyConnection(saved.Connection);
        Alias = saved.Alias;
        SaveConnection = true;
        SavePassword = saved.HasPassword;
    }

    private void ApplyConnection(DirectoryConnection connection)
    {
        Domain = connection.Domain;
        Port = connection.Port;
        User = connection.User;
        Password = connection.Password;
    }

    public void RemoveSavedConnection(SavedDirectoryConnection connection)
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

    public async Task<bool> ConnectAsync()
    {
        ErrorMessage = "";
        var connection = CurrentConnection;
        if (!connection.UsesWindowsCredentials && connection.Password.Length == 0)
        {
            ErrorMessage = "Informe a senha ou deixe o usuário em branco para usar a conta do Windows.";
            return false;
        }

        IsConnecting = true;
        try
        {
            await Task.Run(() => _directory.Test(connection));
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
            var alias = _savedConnections.Find(connection)?.Alias
                        ?? (Alias.Trim().Length > 0 ? Alias.Trim() : connection.ToString());
            var saved = SavePassword ? connection : connection with { Password = "" };
            _savedConnections.Save(new SavedDirectoryConnection(alias, saved));
        }

        Connection = connection;
        return true;
    }

    private static string GetComputerDomain()
    {
        try
        {
            return IPGlobalProperties.GetIPGlobalProperties().DomainName;
        }
        catch (NetworkInformationException)
        {
            return "";
        }
    }
}
