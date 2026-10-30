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

        User = "";
        Password = "";
        DataSource = "";
        DirectDataSource = "";
        Host = "";
        Port = ServerAddress.DefaultPort;
        ServiceName = "";
        Alias = "";
        ErrorMessage = "";

        var current = session.Current;
        var (lastUser, lastPrivilege) = ConnectionInfo.SplitUser(settings.LastLoginUser);
        User = current?.User ?? lastUser;
        PrivilegeIndex = (int)(current?.Privilege ?? lastPrivilege);
        SetDataSource(current?.DataSource ?? settings.LastLoginDataSource ?? "");

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
    [NotifyPropertyChangedFor(nameof(IsTnsMode), nameof(IsDirectMode), nameof(IsBasicMode), nameof(DefaultAlias))]
    public partial ConnectionMode Mode { get; set; }

    public bool IsTnsMode => Mode == ConnectionMode.Tns;

    public bool IsDirectMode => Mode == ConnectionMode.Direct;

    public bool IsBasicMode => Mode == ConnectionMode.Basic;

    // Alias do tnsnames.ora (modo TNS).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string DataSource { get; set; }

    // EZConnect ou descritor completo (modo EZConnect / Descritor).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string DirectDataSource { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string Host { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial double Port { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial string ServiceName { get; set; }

    // 0 = Service name, 1 = SID.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial int ServiceKindIndex { get; set; }

    // Índice de DbaPrivilege (Normal, SYSDBA, SYSOPER).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefaultAlias))]
    public partial int PrivilegeIndex { get; set; }

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

    public string DefaultAlias =>
        new ConnectionInfo(User.Trim(), "", CurrentDataSource(), (DbaPrivilege)PrivilegeIndex).DisplayName;

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

    // Ao trocar entre EZConnect e Básico, leva o endereço de um modo para o outro.
    partial void OnModeChanged(ConnectionMode oldValue, ConnectionMode newValue)
    {
        if (oldValue == ConnectionMode.Basic && newValue == ConnectionMode.Direct && TryBuildBasicAddress(out var address, out _))
        {
            DirectDataSource = address!.ToDataSource();
        }
        else if (oldValue == ConnectionMode.Direct && newValue == ConnectionMode.Basic &&
                 ServerAddress.TryParse(DirectDataSource, out var parsed))
        {
            FillBasicFields(parsed!);
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
        var (user, privilege) = ConnectionInfo.SplitUser(connection.User);
        User = user;
        PrivilegeIndex = (int)privilege;
        Password = connection.Password;
        SetDataSource(connection.DataSource);
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

    // Escolhe o modo pelo formato do texto: vazio ou endereço simples → Básico; EZConnect/descritor → EZConnect; senão alias TNS.
    private void SetDataSource(string dataSource)
    {
        // O modo muda antes dos campos, para OnModeChanged não sobrescrever o texto recebido.
        var text = dataSource.Trim();
        if (text.Length == 0)
        {
            Mode = ConnectionMode.Basic;
        }
        else if (ServerAddress.TryParse(text, out var address))
        {
            Mode = ConnectionMode.Basic;
            FillBasicFields(address!);
            DirectDataSource = text;
        }
        else if (ServerAddress.IsDirect(text))
        {
            Mode = ConnectionMode.Direct;
            DirectDataSource = text;
        }
        else
        {
            Mode = ConnectionMode.Tns;
            DataSource = text;
        }
    }

    private void FillBasicFields(ServerAddress address)
    {
        Host = address.Host;
        Port = address.Port;
        ServiceName = address.ServiceName;
        ServiceKindIndex = address.IsSid ? 1 : 0;
    }

    private bool TryBuildBasicAddress(out ServerAddress? address, out string error)
    {
        address = null;
        var isSid = ServiceKindIndex == 1;
        if (string.IsNullOrWhiteSpace(Host))
        {
            error = "Informe o host.";
            return false;
        }

        if (double.IsNaN(Port) || !ServerAddress.IsValidPort((int)Port))
        {
            error = "Informe uma porta entre 1 e 65535.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            error = isSid ? "Informe o SID." : "Informe o service name.";
            return false;
        }

        address = new ServerAddress(Host.Trim(), (int)Port, ServiceName.Trim(), isSid);
        error = "";
        return true;
    }

    private string CurrentDataSource() => Mode switch
    {
        ConnectionMode.Basic => TryBuildBasicAddress(out var address, out _) ? address!.ToDataSource() : "",
        ConnectionMode.Direct => DirectDataSource.Trim(),
        _ => DataSource.Trim()
    };

    // Retorna true quando conectou (o diálogo pode ser fechado).
    public async Task<bool> ConnectAsync()
    {
        ErrorMessage = "";
        if (Mode == ConnectionMode.Basic && !TryBuildBasicAddress(out _, out var basicError))
        {
            ErrorMessage = basicError;
            return false;
        }

        // Aceita também "SYS AS SYSDBA" digitado no campo usuário.
        var (user, typedPrivilege) = ConnectionInfo.SplitUser(User);
        var privilege = typedPrivilege != DbaPrivilege.Normal ? typedPrivilege : (DbaPrivilege)PrivilegeIndex;
        var connection = new ConnectionInfo(user, Password, CurrentDataSource(), privilege);
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
            var alias = _savedConnections.FindByUserAndDataSource(connection.StoredUser, connection.DataSource)?.Alias
                        ?? (Alias.Trim().Length > 0 ? Alias.Trim() : connection.DisplayName);
            var password = SavePassword ? connection.Password : "";
            _savedConnections.Save(new SavedConnection(alias, connection.StoredUser, password, connection.DataSource));
        }

        return true;
    }
}
