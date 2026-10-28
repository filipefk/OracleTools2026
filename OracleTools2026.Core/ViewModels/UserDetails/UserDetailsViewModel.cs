using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Data;
using OracleTools2026.Formatting;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;
using System.Text;

namespace OracleTools2026.ViewModels.UserDetails;

public sealed partial class UserDetailsViewModel : ToolViewModelBase, IToolArgumentReceiver
{
    private readonly DatabaseUserRepository _repository;
    private readonly UserScriptGenerator _generator;
    private readonly DdlExtractor _ddlExtractor;
    private readonly DdlOutputService _output;
    private readonly ActiveDirectoryService _directory;
    private readonly IAppSettings _settings;
    private DirectoryConnection? _directoryConnection;
    private bool _searchOnActivate;

    public UserDetailsViewModel(ToolServices services, DatabaseUserRepository repository, UserScriptGenerator generator,
        DdlExtractor ddlExtractor, DdlOutputService output, ActiveDirectoryService directory, IAppSettings settings)
        : base(services, "Detalhes do usuário")
    {
        _repository = repository;
        _generator = generator;
        _ddlExtractor = ddlExtractor;
        _output = output;
        _directory = directory;
        _settings = settings;

        UserName = "";
        LocatePattern = "";
        SearchDirectory = true;
        SearchDatabase = true;
        LocateReport = "";
        DirectoryText = "Active Directory: não conectado";
    }

    // A árvore tem um único nó raiz (o usuário pesquisado).
    public ObservableCollection<UserTreeNode> RootNodes { get; } = [];

    public UserTreeNode? RootNode => RootNodes.Count > 0 ? RootNodes[0] : null;

    [ObservableProperty]
    public partial UserTreeNode? SelectedNode { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; }

    [ObservableProperty]
    public partial string LocatePattern { get; set; }

    [ObservableProperty]
    public partial bool SearchDirectory { get; set; }

    [ObservableProperty]
    public partial bool SearchDatabase { get; set; }

    [ObservableProperty]
    public partial string LocateReport { get; private set; }

    [ObservableProperty]
    public partial string DirectoryText { get; private set; }

    // Avisa a tela para selecionar o texto do usuário depois da pesquisa.
    public event EventHandler? UserSearched;

    // Pela linha de comando ou a partir de outra ferramenta: pesquisa o usuário informado. Se a tela já
    // estiver aberta, pesquisa na hora; senão, quando ela for exibida.
    public void SetToolArgument(string argument)
    {
        UserName = argument.ToUpperInvariant();
        if (IsActivated)
        {
            _ = SearchUserAsync();
        }
        else
        {
            _searchOnActivate = true;
        }
    }

    protected override async Task OnActivatedAsync()
    {
        if (_searchOnActivate)
        {
            _searchOnActivate = false;
            await SearchUserAsync();
        }
    }

    // As permissões mostradas são do banco anterior.
    protected override Task OnConnectionChangedAsync()
    {
        SetRoot(null);
        return Task.CompletedTask;
    }

    private void SetRoot(UserTreeNode? root)
    {
        RootNodes.Clear();
        if (root is not null)
        {
            RootNodes.Add(root);
        }

        SelectedNode = root;
        OnPropertyChanged(nameof(RootNode));
    }

    #region Permissões no banco

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task SearchUserAsync()
    {
        var userName = UserName.Trim().ToUpperInvariant();
        if (userName.Length == 0)
        {
            ShowError("Informe o nome do usuário.");
            return;
        }

        Models.UserDetails? details = null;
        IReadOnlyList<RoleGrant> roles = [];
        IReadOnlyList<ObjectPrivilegeGrant> grants = [];
        var completed = await RunAsync($"Pesquisando o usuário {userName}", async (_, ct) =>
        {
            details = await Task.Run(() => _repository.GetDetailsAsync(userName, ct), ct);
            if (details is null)
            {
                return;
            }

            roles = await Task.Run(() => _repository.GetRolesAsync(userName, ct), ct);
            grants = await Task.Run(() => _repository.GetObjectPrivilegesAsync(userName, ct), ct);
        });

        if (!completed)
        {
            return;
        }

        if (details is null)
        {
            SetRoot(null);
            ShowError($"Usuário {userName} não encontrado.");
            return;
        }

        SetRoot(BuildTree(details, roles, grants));
        UserName = userName;
        UserSearched?.Invoke(this, EventArgs.Empty);
        StatusText = $"Usuário {userName} pesquisado às {Now()}";
    }

    private static UserTreeNode BuildTree(Models.UserDetails details, IReadOnlyList<RoleGrant> roles, IReadOnlyList<ObjectPrivilegeGrant> grants)
    {
        var root = new UserTreeNode(details.Name, UserTreeNodeKind.User, details.Name);
        root.AddInfo($"STATUS: {details.Status}");
        root.AddInfo($"DEFAULT TABLESPACE: {details.DefaultTablespace}");
        root.AddInfo($"TEMPORARY TABLESPACE: {details.TemporaryTablespace}");
        root.AddLazy($"PROFILE: {details.Profile}", UserTreeNodeKind.Profile, details.Profile);
        root.AddInfo($"CRIADO EM: {DisplayFormat.Date(details.CreatedAt)}");
        if (details.LockDate is not null)
        {
            root.AddInfo($"DATA DE LOCK: {DisplayFormat.Date(details.LockDate)}");
        }

        if (details.ExpiryDate is not null)
        {
            root.AddInfo($"DATA DE EXPIRAÇÃO: {DisplayFormat.Date(details.ExpiryDate)}");
        }

        root.AddInfo($"ÚLTIMO LOGIN: {(details.LastLogin is null ? "Sem registro" : DisplayFormat.Date(details.LastLogin))}");
        root.AddLazy("Tablespaces", UserTreeNodeKind.Quotas, details.Name);
        root.AddLazy("Privilégios", UserTreeNodeKind.SystemPrivileges, details.Name);

        var rolesNode = root.AddInfo("Roles");
        foreach (var role in roles)
        {
            var text = $"{role.Role}: {(role.IsDefault ? "DEFAULT" : "NÃO DEFAULT")} {(role.AdminOption ? "COM" : "SEM")} ADMIN OPTION";
            rolesNode.AddLazy(text, UserTreeNodeKind.Role, role.Role);
        }

        rolesNode.AddEmptyMarker("Nenhuma role");

        var grantsNode = root.AddInfo("Grants");
        AddObjectGrantNodes(grantsNode, grants);
        grantsNode.AddEmptyMarker("Nenhum grant direto ao usuário");

        rolesNode.IsExpanded = true;
        grantsNode.IsExpanded = true;
        root.IsExpanded = true;
        return root;
    }

    // Agrupa os grants por tipo de objeto e, dentro dele, por objeto: "SELECT, UPDATE ON OWNER.NOME".
    private static void AddObjectGrantNodes(UserTreeNode parent, IReadOnlyList<ObjectPrivilegeGrant> grants)
    {
        foreach (var typeGroup in grants.GroupBy(g => g.ObjectType.Length > 0 ? g.ObjectType : "OUTROS"))
        {
            var typeNode = parent.AddInfo(typeGroup.Key);
            foreach (var objectGroup in typeGroup.GroupBy(g => g.QualifiedName))
            {
                var first = objectGroup.First();
                typeNode.Add(new UserTreeNode(FormatGrant(objectGroup.ToList()), UserTreeNodeKind.ObjectGrant, first.QualifiedName, first));
            }
        }
    }

    private static string FormatGrant(IReadOnlyList<ObjectPrivilegeGrant> grants)
    {
        var tablePrivileges = grants
            .Where(g => g.Column is null)
            .Select(g => g.Grantable ? $"{g.Privilege} WITH GRANT OPTION" : g.Privilege);

        var columnPrivileges = grants
            .Where(g => g.Column is not null)
            .GroupBy(g => (g.Privilege, g.Grantable))
            .Select(g => $"{g.Key.Privilege} ({string.Join(", ", g.Select(c => c.Column))}){(g.Key.Grantable ? " WITH GRANT OPTION" : "")}");

        var first = grants[0];
        var target = first.IsDirectory ? $"DIRECTORY {first.ObjectName}" : first.QualifiedName;
        return $"{string.Join(", ", tablePrivileges.Concat(columnPrivileges))} ON {target}";
    }

    private static string FormatSystemPrivilege(SystemPrivilegeGrant privilege) =>
        privilege.AdminOption ? $"{privilege.Privilege} WITH ADMIN OPTION" : privilege.Privilege;

    // Chamado pela tela ao expandir um nó sob demanda.
    public async Task ExpandNodeAsync(UserTreeNode node)
    {
        if (!node.IsLazy)
        {
            return;
        }

        if (IsBusy || !await RunAsync($"Carregando {node.Text}", (_, ct) => LoadChildrenAsync(node, ct)))
        {
            node.IsExpanded = false;
            return;
        }

        node.IsExpanded = true;
    }

    private async Task LoadChildrenAsync(UserTreeNode node, CancellationToken cancellationToken)
    {
        var children = new UserTreeNode("");
        switch (node.Kind)
        {
            case UserTreeNodeKind.Profile:
                var limits = await Task.Run(() => _repository.GetProfileLimitsAsync(node.Name, cancellationToken), cancellationToken);
                foreach (var group in limits.GroupBy(l => l.ResourceType))
                {
                    var typeNode = children.AddInfo(group.Key);
                    foreach (var limit in group)
                    {
                        typeNode.AddInfo($"{limit.ResourceName} = {limit.Limit}{(limit.InheritedFromDefault ? " (DEFAULT)" : "")}");
                    }
                }

                children.AddEmptyMarker("Profile sem limites");
                break;

            case UserTreeNodeKind.Quotas:
                foreach (var quota in await Task.Run(() => _repository.GetQuotasAsync(node.Name, cancellationToken), cancellationToken))
                {
                    var limit = quota.IsUnlimited ? "UNLIMITED" : DisplayFormat.Bytes(quota.MaxBytes!.Value);
                    children.AddInfo($"{quota.Tablespace}: {limit} (em uso: {DisplayFormat.Bytes(quota.UsedBytes)})");
                }

                children.AddEmptyMarker("Nenhuma cota");
                break;

            case UserTreeNodeKind.SystemPrivileges:
                foreach (var privilege in await Task.Run(() => _repository.GetSystemPrivilegesAsync(node.Name, cancellationToken), cancellationToken))
                {
                    children.AddInfo(FormatSystemPrivilege(privilege));
                }

                children.AddEmptyMarker("Nenhum privilégio de sistema");
                break;

            case UserTreeNodeKind.Role:
                var privileges = await Task.Run(() => _repository.GetSystemPrivilegesAsync(node.Name, cancellationToken), cancellationToken);
                var nestedRoles = await Task.Run(() => _repository.GetRolesAsync(node.Name, cancellationToken), cancellationToken);
                var grants = await Task.Run(() => _repository.GetObjectPrivilegesAsync(node.Name, cancellationToken), cancellationToken);

                foreach (var privilege in privileges)
                {
                    children.AddInfo(FormatSystemPrivilege(privilege));
                }

                foreach (var role in nestedRoles)
                {
                    children.AddLazy($"ROLE {role.Role}{(role.AdminOption ? " WITH ADMIN OPTION" : "")}", UserTreeNodeKind.Role, role.Role);
                }

                AddObjectGrantNodes(children, grants);
                children.AddEmptyMarker("Role sem privilégios");
                break;
        }

        node.Children.Clear();
        foreach (var child in children.Children)
        {
            node.Children.Add(child);
        }

        node.IsLazy = false;
    }

    // Carrega todos os nós sob demanda (inclusive os que aparecem ao carregar outros, como roles de roles)
    // e expande a árvore inteira.
    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ExpandAllAsync()
    {
        if (RootNode is not { } root)
        {
            ShowInfo("Pesquise um usuário primeiro.");
            return;
        }

        var completed = await RunAsync("Carregando todos os detalhes", async (progress, ct) =>
        {
            List<UserTreeNode> pending;
            while ((pending = root.FindLazyNodes().ToList()).Count > 0)
            {
                for (var i = 0; i < pending.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(new ProgressInfo(i + 1, pending.Count, $"Carregando {pending[i].Text}"));
                    await LoadChildrenAsync(pending[i], ct);
                }
            }
        });

        if (completed)
        {
            root.ExpandAll();
        }
    }

    // Mesmo formato do original: dados do banco e os nós expandidos, indentados com tabulação.
    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task GenerateReportAsync()
    {
        if (RootNode is not { } root)
        {
            ShowInfo("Pesquise um usuário primeiro.");
            return;
        }

        var serverInfo = "";
        if (!await RunAsync("Gerando relatório", async (_, ct) =>
                serverInfo = await Task.Run(() => Services.Repository.GetServerInfoAsync(ct), ct)))
        {
            return;
        }

        var report = new StringBuilder();
        report.AppendLine(serverInfo);
        report.AppendLine(root.Text);
        if (root.IsExpanded)
        {
            AppendReportNodes(report, root, 1);
        }

        await OpenInEditorAsync(report.ToString(), $"Relatorio_Usuario_{root.Text}.txt");
    }

    private static void AppendReportNodes(StringBuilder report, UserTreeNode parent, int level)
    {
        foreach (var node in parent.Children)
        {
            report.Append('\t', level).AppendLine(node.Text);
            if (node.IsExpanded)
            {
                AppendReportNodes(report, node, level + 1);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ExtractUserScriptAsync()
    {
        var userName = (RootNode?.Text ?? UserName).Trim().ToUpperInvariant();
        if (userName.Length == 0)
        {
            ShowError("Informe o nome do usuário.");
            return;
        }

        UserScriptResult? result = null;
        await RunAsync($"Extraindo script do usuário {userName}", async (_, ct) =>
            result = await Task.Run(() => _generator.GenerateAsync(userName, ct), ct));

        if (result is null)
        {
            return;
        }

        if (result.Succeeded)
        {
            await OpenInEditorAsync(result.Script, result.FileName);
        }
        else
        {
            ShowError(result.Error ?? "Script não gerado.");
        }
    }

    // Grants de objetos cujo tipo tem DDL extraível.
    public static bool CanOpenDdl(UserTreeNode? node) =>
        node is { Kind: UserTreeNodeKind.ObjectGrant, Grant: { } grant } &&
        DatabaseObjectRepository.SupportedTypes.Contains(grant.ObjectType);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task OpenObjectDdlAsync(UserTreeNode? node)
    {
        if (!CanOpenDdl(node))
        {
            return;
        }

        var qualifiedName = node!.Grant!.QualifiedName;
        var options = new DdlExtractionOptions(_settings.IncludeHeaderOnDdlExtraction, _settings.InlineCompileErrorsOnDdlExtraction);
        DdlExtractionResult? result = null;
        await RunAsync($"Extraindo DDL do objeto {qualifiedName}", async (_, ct) =>
            result = await Task.Run(() => _ddlExtractor.ExtractAsync(qualifiedName, options, ct), ct));

        if (result is null)
        {
            return;
        }

        if (result.Succeeded)
        {
            await OpenInEditorAsync(result.Source, result.FileName);
        }
        else
        {
            ShowError(result.Error ?? "DDL não encontrada.");
        }
    }

    [RelayCommand]
    private void CopyNodeText(UserTreeNode? node)
    {
        if ((node ?? SelectedNode) is { } target)
        {
            Clipboard.SetText(target.Text);
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

    #endregion

    #region Localizar usuário

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task ConnectDirectoryAsync() => await ShowDirectoryLoginAsync();

    private async Task<DirectoryConnection?> ShowDirectoryLoginAsync()
    {
        if (await Host.ShowDirectoryLoginAsync(_directoryConnection) is not { } connection)
        {
            return null;
        }

        _directoryConnection = connection;
        DirectoryText = $"Active Directory: {connection}";
        return connection;
    }

    // Procura o usuário no Active Directory e no banco (DBA_USERS).
    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task LocateUserAsync()
    {
        var pattern = LocatePattern.Trim();
        if (pattern.Length == 0)
        {
            ShowError("Informe o nome ou o login a procurar.");
            return;
        }

        var searchDirectory = SearchDirectory;
        var searchDatabase = SearchDatabase;
        if (!searchDirectory && !searchDatabase)
        {
            ShowError("Marque onde procurar: Active Directory e/ou banco de dados.");
            return;
        }

        var directoryConnection = searchDirectory ? _directoryConnection ?? await ShowDirectoryLoginAsync() : null;
        var databaseConnected = searchDatabase && await EnsureConnectedAsync();

        var report = new StringBuilder();
        var completed = await RunAsync($"Procurando {pattern}", async (_, ct) =>
        {
            if (searchDirectory)
            {
                report.AppendLine("######## Procurando no Active Directory: ########").AppendLine();
                if (directoryConnection is null)
                {
                    report.AppendLine("Busca não realizada: sem conexão com o Active Directory.");
                }
                else
                {
                    try
                    {
                        AppendDirectoryUsers(report, pattern, await Task.Run(() => _directory.FindUsers(directoryConnection, pattern, ct), ct));
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        report.AppendLine($"Erro ao consultar o Active Directory: {ex.Message}");
                    }
                }

                report.AppendLine().AppendLine();
            }

            if (searchDatabase)
            {
                report.AppendLine("######## Procurando no banco de dados: ########").AppendLine();
                if (!databaseConnected)
                {
                    report.AppendLine("Busca não realizada: sem conexão com o banco de dados.");
                }
                else
                {
                    var users = await Task.Run(() => _repository.FindAsync(pattern, ct), ct);
                    AppendDatabaseUsers(report, pattern, users);
                }
            }
        }, requiresConnection: false);

        if (completed)
        {
            LocateReport = report.ToString();
            StatusText = $"Busca por {pattern} concluída às {Now()}";
        }
    }

    private static void AppendDirectoryUsers(StringBuilder report, string pattern, IReadOnlyList<DirectoryUser> users)
    {
        if (users.Count == 0)
        {
            report.AppendLine($"Nenhum resultado encontrado no AD para {pattern}");
            return;
        }

        foreach (var user in users)
        {
            var name = user.DisplayName.Length > 0 ? user.DisplayName : user.Description;
            report.AppendLine($"{user.Login} - {(name.Length > 0 ? name.ToUpperInvariant() : "(sem nome no AD)")}");
            AppendIfPresent(report, "Descrição", user.Description);
            AppendIfPresent(report, "E-mail", user.Email);
            AppendIfPresent(report, "Departamento", user.Department);
            AppendIfPresent(report, "Cargo", user.Title);
            report.AppendLine($"Conta desabilitada: {YesNo(user.Disabled)}");
            report.AppendLine($"Conta bloqueada: {YesNo(user.LockedOut)}");
            report.AppendLine($"Senha nunca expira: {YesNo(user.PasswordNeverExpires)}");
            report.AppendLine($"Trocar senha no próximo logon: {YesNo(user.MustChangePassword)}");
            AppendIfPresent(report, "Senha alterada em", DisplayFormat.Date(user.PasswordLastSet));
            AppendIfPresent(report, "Último logon (aproximado)", DisplayFormat.Date(user.LastLogon));
            AppendIfPresent(report, "Conta expira em", DisplayFormat.Date(user.AccountExpires));
            report.AppendLine("Membro dos grupos:");
            foreach (var group in user.Groups)
            {
                report.AppendLine($"- {group}");
            }

            report.AppendLine();
        }
    }

    private static void AppendDatabaseUsers(StringBuilder report, string pattern, IReadOnlyList<DatabaseUser> users)
    {
        report.AppendLine("Procurando em DBA_USERS:").AppendLine();
        if (users.Count == 0)
        {
            report.AppendLine($"Nenhum usuário com nome {pattern} encontrado");
        }

        foreach (var user in users)
        {
            report.AppendLine($"Nome: {user.Name} - Status: {user.Status} - Criado em: {DisplayFormat.Date(user.CreatedAt)} - Profile: {user.Profile}");
        }
    }

    private static void AppendIfPresent(StringBuilder report, string label, string value)
    {
        if (value.Length > 0)
        {
            report.AppendLine($"{label}: {value}");
        }
    }

    private static string YesNo(bool value) => value ? "Sim" : "Não";

    #endregion
}
