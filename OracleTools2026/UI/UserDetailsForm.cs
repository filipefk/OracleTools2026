using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Globalization;
using System.Text;

namespace OracleTools2026.UI;

public partial class UserDetailsForm : DatabaseToolForm, IToolArgumentReceiver
{
    private const string LoadingText = "Carregando...";

    // Tag do nó filho provisório dos nós carregados sob demanda.
    private static readonly object LazyPlaceholder = new();

    private readonly DatabaseUserRepository _repository;
    private readonly DatabaseObjectRepository _objectRepository;
    private readonly UserScriptGenerator _generator;
    private readonly DdlExtractor _ddlExtractor;
    private readonly DdlOutputService _output;
    private readonly ActiveDirectoryService _directory;
    private readonly IAppSettings _settings;
    private readonly Func<DirectoryLoginForm> _directoryLoginFormFactory;

    private DirectoryConnection? _directoryConnection;
    private bool _searchOnLoad;

    public UserDetailsForm(
        DatabaseToolDependencies dependencies,
        DatabaseUserRepository repository,
        DatabaseObjectRepository objectRepository,
        UserScriptGenerator generator,
        DdlExtractor ddlExtractor,
        DdlOutputService output,
        ActiveDirectoryService directory,
        IAppSettings settings,
        Func<DirectoryLoginForm> directoryLoginFormFactory)
        : base(dependencies, "Detalhes do usuário")
    {
        _repository = repository;
        _objectRepository = objectRepository;
        _generator = generator;
        _ddlExtractor = ddlExtractor;
        _output = output;
        _directory = directory;
        _settings = settings;
        _directoryLoginFormFactory = directoryLoginFormFactory;

        InitializeComponent();
        BindStatus(statusLabel, progressBar);
    }

    public void SetToolArgument(string argument)
    {
        userTextBox.Text = argument.ToUpperInvariant();
        _searchOnLoad = true;
    }

    private async void UserDetailsForm_Load(object? sender, EventArgs e)
    {
        UpdateDirectoryLabel();
        if (_searchOnLoad)
        {
            await SearchUserAsync();
        }
    }

    // As permissões mostradas são do banco anterior.
    protected override void OnConnectionChanged() => userTreeView.Nodes.Clear();

    protected override void OnBusyChanged(bool busy)
    {
        Control[] controls =
        [
            userTextBox, searchUserButton, expandAllButton, reportButton, extractScriptButton,
            locateTextBox, searchDirectoryCheckBox, searchDatabaseCheckBox, locateButton
        ];

        foreach (var control in controls)
        {
            control.Enabled = !busy;
        }

        fileMenu.Enabled = !busy;
        cancelUserButton.Enabled = busy;
        cancelLocateButton.Enabled = busy;
    }

    private TreeNode? RootNode => userTreeView.Nodes.Count > 0 ? userTreeView.Nodes[0] : null;

    #region Menu

    private void ConnectMenuItem_Click(object? sender, EventArgs e) => ShowLogin();

    private void DirectoryConnectMenuItem_Click(object? sender, EventArgs e) => ShowDirectoryLogin();

    private async void ServerInfoMenuItem_Click(object? sender, EventArgs e) => await ShowServerInfoAsync();

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();

    private void CancelButton_Click(object? sender, EventArgs e) => CancelOperation();

    #endregion

    #region Permissões no banco

    private async void SearchUserButton_Click(object? sender, EventArgs e) => await SearchUserAsync();

    private async void UserTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await SearchUserAsync();
        }
    }

    private async void ExpandAllButton_Click(object? sender, EventArgs e) => await ExpandAllAsync();

    private async void ReportButton_Click(object? sender, EventArgs e) => await GenerateReportAsync();

    private async void ExtractScriptButton_Click(object? sender, EventArgs e) => await ExtractUserScriptAsync();

    private async Task SearchUserAsync()
    {
        var userName = userTextBox.Text.Trim().ToUpperInvariant();
        if (userName.Length == 0)
        {
            ShowError("Informe o nome do usuário.");
            return;
        }

        UserDetails? details = null;
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
            userTreeView.Nodes.Clear();
            ShowError($"Usuário {userName} não encontrado.");
            return;
        }

        BuildTree(details, roles, grants);
        userTextBox.Text = userName;
        userTextBox.SelectAll();
        SetStatus($"Usuário {userName} pesquisado às {DateTime.Now:HH:mm:ss}");
    }

    private void BuildTree(UserDetails details, IReadOnlyList<RoleGrant> roles, IReadOnlyList<ObjectPrivilegeGrant> grants)
    {
        var root = new TreeNode(details.Name) { Tag = new UserTreeNodeInfo(UserTreeNodeKind.User, details.Name) };

        AddInfo(root, $"STATUS: {details.Status}");
        AddInfo(root, $"DEFAULT TABLESPACE: {details.DefaultTablespace}");
        AddInfo(root, $"TEMPORARY TABLESPACE: {details.TemporaryTablespace}");
        AddLazy(root, $"PROFILE: {details.Profile}", new UserTreeNodeInfo(UserTreeNodeKind.Profile, details.Profile));
        AddInfo(root, $"CRIADO EM: {ListViewExtensions.FormatDate(details.CreatedAt)}");

        if (details.LockDate is not null)
        {
            AddInfo(root, $"DATA DE LOCK: {ListViewExtensions.FormatDate(details.LockDate)}");
        }

        if (details.ExpiryDate is not null)
        {
            AddInfo(root, $"DATA DE EXPIRAÇÃO: {ListViewExtensions.FormatDate(details.ExpiryDate)}");
        }

        AddInfo(root, $"ÚLTIMO LOGIN: {(details.LastLogin is null ? "Sem registro" : ListViewExtensions.FormatDate(details.LastLogin))}");

        AddLazy(root, "Tablespaces", new UserTreeNodeInfo(UserTreeNodeKind.Quotas, details.Name));
        AddLazy(root, "Privilégios", new UserTreeNodeInfo(UserTreeNodeKind.SystemPrivileges, details.Name));

        var rolesNode = AddInfo(root, "Roles");
        foreach (var role in roles)
        {
            var text = $"{role.Role}: {(role.IsDefault ? "DEFAULT" : "NÃO DEFAULT")} {(role.AdminOption ? "COM" : "SEM")} ADMIN OPTION";
            AddLazy(rolesNode, text, new UserTreeNodeInfo(UserTreeNodeKind.Role, role.Role));
        }

        AddEmptyMarker(rolesNode, "Nenhuma role");

        var grantsNode = AddInfo(root, "Grants");
        AddObjectGrantNodes(grantsNode, grants);
        AddEmptyMarker(grantsNode, "Nenhum grant direto ao usuário");

        userTreeView.BeginUpdate();
        userTreeView.Nodes.Clear();
        userTreeView.Nodes.Add(root);
        rolesNode.Expand();
        grantsNode.Expand();
        root.Expand();
        userTreeView.EndUpdate();
        userTreeView.SelectedNode = root;
    }

    private static TreeNode AddInfo(TreeNode parent, string text)
    {
        var node = new TreeNode(text) { Tag = UserTreeNodeInfo.Info };
        parent.Nodes.Add(node);
        return node;
    }

    private static void AddLazy(TreeNode parent, string text, UserTreeNodeInfo info)
    {
        var node = new TreeNode(text) { Tag = info };
        node.Nodes.Add(new TreeNode(LoadingText) { Tag = LazyPlaceholder });
        parent.Nodes.Add(node);
    }

    private static void AddEmptyMarker(TreeNode node, string text)
    {
        if (node.Nodes.Count == 0)
        {
            node.Nodes.Add(new TreeNode($"({text})") { Tag = UserTreeNodeInfo.Info, ForeColor = SystemColors.GrayText });
        }
    }

    // Agrupa os grants por tipo de objeto e, dentro dele, por objeto: "SELECT, UPDATE ON OWNER.NOME".
    private static void AddObjectGrantNodes(TreeNode parent, IReadOnlyList<ObjectPrivilegeGrant> grants)
    {
        foreach (var typeGroup in grants.GroupBy(g => g.ObjectType.Length > 0 ? g.ObjectType : "OUTROS"))
        {
            var typeNode = AddInfo(parent, typeGroup.Key);
            foreach (var objectGroup in typeGroup.GroupBy(g => g.QualifiedName))
            {
                var first = objectGroup.First();
                typeNode.Nodes.Add(new TreeNode(FormatGrant(objectGroup.ToList()))
                {
                    Tag = new UserTreeNodeInfo(UserTreeNodeKind.ObjectGrant, first.QualifiedName, first)
                });
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

    private static bool IsLazy(TreeNode node) =>
        node.Nodes.Count == 1 && ReferenceEquals(node.Nodes[0].Tag, LazyPlaceholder);

    private async void UserTreeView_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node is not { } node || !IsLazy(node))
        {
            return;
        }

        e.Cancel = true;
        if (IsBusy)
        {
            return;
        }

        if (await RunAsync($"Carregando {node.Text}", (_, ct) => LoadChildrenAsync(node, ct)))
        {
            node.Expand();
        }
    }

    private async Task LoadChildrenAsync(TreeNode node, CancellationToken cancellationToken)
    {
        var info = (UserTreeNodeInfo)node.Tag!;
        var children = new TreeNode("");

        switch (info.Kind)
        {
            case UserTreeNodeKind.Profile:
                var limits = await Task.Run(() => _repository.GetProfileLimitsAsync(info.Name, cancellationToken), cancellationToken);
                foreach (var group in limits.GroupBy(l => l.ResourceType))
                {
                    var typeNode = AddInfo(children, group.Key);
                    foreach (var limit in group)
                    {
                        AddInfo(typeNode, $"{limit.ResourceName} = {limit.Limit}{(limit.InheritedFromDefault ? " (DEFAULT)" : "")}");
                    }
                }

                AddEmptyMarker(children, "Profile sem limites");
                break;

            case UserTreeNodeKind.Quotas:
                foreach (var quota in await Task.Run(() => _repository.GetQuotasAsync(info.Name, cancellationToken), cancellationToken))
                {
                    var limit = quota.IsUnlimited ? "UNLIMITED" : FormatBytes(quota.MaxBytes!.Value);
                    AddInfo(children, $"{quota.Tablespace}: {limit} (em uso: {FormatBytes(quota.UsedBytes)})");
                }

                AddEmptyMarker(children, "Nenhuma cota");
                break;

            case UserTreeNodeKind.SystemPrivileges:
                foreach (var privilege in await Task.Run(() => _repository.GetSystemPrivilegesAsync(info.Name, cancellationToken), cancellationToken))
                {
                    AddInfo(children, FormatSystemPrivilege(privilege));
                }

                AddEmptyMarker(children, "Nenhum privilégio de sistema");
                break;

            case UserTreeNodeKind.Role:
                var privileges = await Task.Run(() => _repository.GetSystemPrivilegesAsync(info.Name, cancellationToken), cancellationToken);
                var nestedRoles = await Task.Run(() => _repository.GetRolesAsync(info.Name, cancellationToken), cancellationToken);
                var grants = await Task.Run(() => _repository.GetObjectPrivilegesAsync(info.Name, cancellationToken), cancellationToken);

                foreach (var privilege in privileges)
                {
                    AddInfo(children, FormatSystemPrivilege(privilege));
                }

                foreach (var role in nestedRoles)
                {
                    AddLazy(children, $"ROLE {role.Role}{(role.AdminOption ? " WITH ADMIN OPTION" : "")}",
                        new UserTreeNodeInfo(UserTreeNodeKind.Role, role.Role));
                }

                AddObjectGrantNodes(children, grants);
                AddEmptyMarker(children, "Role sem privilégios");
                break;
        }

        var nodes = children.Nodes.Cast<TreeNode>().ToArray();
        children.Nodes.Clear();

        userTreeView.BeginUpdate();
        node.Nodes.Clear();
        node.Nodes.AddRange(nodes);
        userTreeView.EndUpdate();
    }

    private static string FormatSystemPrivilege(SystemPrivilegeGrant privilege) =>
        privilege.AdminOption ? $"{privilege.Privilege} WITH ADMIN OPTION" : privilege.Privilege;

    private static string FormatBytes(long bytes)
    {
        string[] units = ["bytes", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} bytes" : $"{value.ToString("0.##", CultureInfo.CurrentCulture)} {units[unit]}";
    }

    // Carrega todos os nós sob demanda (inclusive os que aparecem ao carregar outros, como roles de roles)
    // e expande a árvore inteira.
    private async Task ExpandAllAsync()
    {
        if (RootNode is not { } root)
        {
            ShowInfo("Pesquise um usuário primeiro.");
            return;
        }

        var completed = await RunAsync("Carregando todos os detalhes", async (progress, ct) =>
        {
            List<TreeNode> pending;
            while ((pending = FindLazyNodes(root).ToList()).Count > 0)
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
            userTreeView.BeginUpdate();
            root.ExpandAll();
            userTreeView.EndUpdate();
            root.EnsureVisible();
        }
    }

    private static IEnumerable<TreeNode> FindLazyNodes(TreeNode node)
    {
        if (IsLazy(node))
        {
            yield return node;
            yield break;
        }

        foreach (TreeNode child in node.Nodes)
        {
            foreach (var lazy in FindLazyNodes(child))
            {
                yield return lazy;
            }
        }
    }

    // Mesmo formato do original: dados do banco e os nós expandidos, indentados com tabulação.
    private async Task GenerateReportAsync()
    {
        if (RootNode is not { } root)
        {
            ShowInfo("Pesquise um usuário primeiro.");
            return;
        }

        var serverInfo = "";
        if (!await RunAsync("Gerando relatório", async (_, ct) =>
                serverInfo = await Task.Run(() => _objectRepository.GetServerInfoAsync(ct), ct)))
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

        await _output.OpenInEditorAsync(report.ToString(), $"Relatorio_Usuario_{root.Text}.txt");
    }

    private static void AppendReportNodes(StringBuilder report, TreeNode parent, int level)
    {
        foreach (TreeNode node in parent.Nodes)
        {
            if (ReferenceEquals(node.Tag, LazyPlaceholder))
            {
                continue;
            }

            report.Append('\t', level).AppendLine(node.Text);
            if (node.IsExpanded)
            {
                AppendReportNodes(report, node, level + 1);
            }
        }
    }

    private async Task ExtractUserScriptAsync()
    {
        var userName = (RootNode?.Text ?? userTextBox.Text).Trim().ToUpperInvariant();
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
            await _output.OpenInEditorAsync(result.Script, result.FileName);
        }
        else
        {
            ShowError(result.Error ?? "Script não gerado.");
        }
    }

    private void UserTreeView_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            userTreeView.SelectedNode = e.Node;
        }
    }

    private void UserTreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C && userTreeView.SelectedNode is { } node)
        {
            _output.CopyToClipboard(node.Text);
            e.SuppressKeyPress = true;
        }
    }

    private void TreeContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        treeContextMenu.Items.Clear();

        if (userTreeView.SelectedNode is { } node)
        {
            if (node.Tag is UserTreeNodeInfo { Kind: UserTreeNodeKind.ObjectGrant, Grant: { } grant } &&
                DatabaseObjectRepository.SupportedTypes.Contains(grant.ObjectType))
            {
                treeContextMenu.Items.Add($"Abrir DDL de {grant.QualifiedName} no editor", null,
                    async (_, _) => await OpenObjectDdlAsync(grant.QualifiedName));
                treeContextMenu.Items.Add(new ToolStripSeparator());
            }

            treeContextMenu.Items.Add("Copiar texto", null, (_, _) => _output.CopyToClipboard(node.Text));
        }

        if (RootNode is not null)
        {
            treeContextMenu.Items.Add("Expandir tudo", null, async (_, _) => await ExpandAllAsync());
            treeContextMenu.Items.Add("Gerar relatório", null, async (_, _) => await GenerateReportAsync());
            treeContextMenu.Items.Add("Extrair script do usuário", null, async (_, _) => await ExtractUserScriptAsync());
        }

        e.Cancel = IsBusy || treeContextMenu.Items.Count == 0;
    }

    private async Task OpenObjectDdlAsync(string qualifiedName)
    {
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
            await _output.OpenInEditorAsync(result.Source, result.FileName);
        }
        else
        {
            ShowError(result.Error ?? "DDL não encontrada.");
        }
    }

    #endregion

    #region Localizar usuário

    private async void LocateButton_Click(object? sender, EventArgs e) => await LocateUserAsync();

    private async void LocateTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await LocateUserAsync();
        }
    }

    private void ReportTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            reportTextBox.SelectAll();
            e.SuppressKeyPress = true;
        }
    }

    // Procura o usuário no Active Directory e no banco (DBA_USERS).
    private async Task LocateUserAsync()
    {
        var pattern = locateTextBox.Text.Trim();
        if (pattern.Length == 0)
        {
            ShowError("Informe o nome ou o login a procurar.");
            return;
        }

        if (!searchDirectoryCheckBox.Checked && !searchDatabaseCheckBox.Checked)
        {
            ShowError("Marque onde procurar: Active Directory e/ou banco de dados.");
            return;
        }

        var directoryConnection = searchDirectoryCheckBox.Checked ? _directoryConnection ?? ShowDirectoryLogin() : null;
        var searchDatabase = searchDatabaseCheckBox.Checked && EnsureConnected();

        var report = new StringBuilder();
        var completed = await RunAsync($"Procurando {pattern}", async (_, ct) =>
        {
            if (searchDirectoryCheckBox.Checked)
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

            if (searchDatabaseCheckBox.Checked)
            {
                report.AppendLine("######## Procurando no banco de dados: ########").AppendLine();
                if (!searchDatabase)
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
            reportTextBox.Text = report.ToString();
            SetStatus($"Busca por {pattern} concluída às {DateTime.Now:HH:mm:ss}");
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
            AppendIfPresent(report, "Senha alterada em", ListViewExtensions.FormatDate(user.PasswordLastSet));
            AppendIfPresent(report, "Último logon (aproximado)", ListViewExtensions.FormatDate(user.LastLogon));
            AppendIfPresent(report, "Conta expira em", ListViewExtensions.FormatDate(user.AccountExpires));
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
            report.AppendLine($"Nome: {user.Name} - Status: {user.Status} - Criado em: {ListViewExtensions.FormatDate(user.CreatedAt)} - Profile: {user.Profile}");
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

    private DirectoryConnection? ShowDirectoryLogin()
    {
        using var form = _directoryLoginFormFactory().WithConnection(_directoryConnection);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _directoryConnection = form.Connection;
            UpdateDirectoryLabel();
        }

        return form.DialogResult == DialogResult.OK ? _directoryConnection : null;
    }

    private void UpdateDirectoryLabel() =>
        directoryLabel.Text = _directoryConnection is { } connection
            ? $"Active Directory: {connection}"
            : "Active Directory: não conectado";

    #endregion
}
