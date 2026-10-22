namespace OracleTools2026.UI;

partial class UserDetailsForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        connectMenuItem = new ToolStripMenuItem();
        directoryConnectMenuItem = new ToolStripMenuItem();
        serverInfoMenuItem = new ToolStripMenuItem();
        fileSeparator = new ToolStripSeparator();
        exitMenuItem = new ToolStripMenuItem();
        tabControl = new TabControl();
        permissionsTab = new TabPage();
        userTreeView = new TreeView();
        treeContextMenu = new ContextMenuStrip(components);
        userToolbarPanel = new FlowLayoutPanel();
        userLabel = new Label();
        userTextBox = new TextBox();
        searchUserButton = new Button();
        expandAllButton = new Button();
        reportButton = new Button();
        extractScriptButton = new Button();
        cancelUserButton = new Button();
        locateTab = new TabPage();
        reportTextBox = new TextBox();
        locateToolbarPanel = new FlowLayoutPanel();
        locateLabel = new Label();
        locateTextBox = new TextBox();
        searchDirectoryCheckBox = new CheckBox();
        searchDatabaseCheckBox = new CheckBox();
        locateButton = new Button();
        cancelLocateButton = new Button();
        directoryLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        menuStrip.SuspendLayout();
        tabControl.SuspendLayout();
        permissionsTab.SuspendLayout();
        userToolbarPanel.SuspendLayout();
        locateTab.SuspendLayout();
        locateToolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // menuStrip
        //
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(984, 24);
        menuStrip.TabIndex = 0;
        //
        // fileMenu
        //
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { connectMenuItem, directoryConnectMenuItem, serverInfoMenuItem, fileSeparator, exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(61, 20);
        fileMenu.Text = "&Arquivo";
        //
        // connectMenuItem
        //
        connectMenuItem.Name = "connectMenuItem";
        connectMenuItem.ShortcutKeys = Keys.Control | Keys.L;
        connectMenuItem.Size = new Size(260, 22);
        connectMenuItem.Text = "&Conectar...";
        connectMenuItem.Click += ConnectMenuItem_Click;
        //
        // directoryConnectMenuItem
        //
        directoryConnectMenuItem.Name = "directoryConnectMenuItem";
        directoryConnectMenuItem.Size = new Size(260, 22);
        directoryConnectMenuItem.Text = "Conectar no &Active Directory...";
        directoryConnectMenuItem.Click += DirectoryConnectMenuItem_Click;
        //
        // serverInfoMenuItem
        //
        serverInfoMenuItem.Name = "serverInfoMenuItem";
        serverInfoMenuItem.Size = new Size(260, 22);
        serverInfoMenuItem.Text = "&Informações do banco";
        serverInfoMenuItem.Click += ServerInfoMenuItem_Click;
        //
        // fileSeparator
        //
        fileSeparator.Name = "fileSeparator";
        fileSeparator.Size = new Size(257, 6);
        //
        // exitMenuItem
        //
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(260, 22);
        exitMenuItem.Text = "Sai&r";
        exitMenuItem.Click += ExitMenuItem_Click;
        //
        // tabControl
        //
        tabControl.Controls.Add(permissionsTab);
        tabControl.Controls.Add(locateTab);
        tabControl.Dock = DockStyle.Fill;
        tabControl.Location = new Point(0, 24);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(984, 615);
        tabControl.TabIndex = 1;
        //
        // permissionsTab
        //
        permissionsTab.Controls.Add(userTreeView);
        permissionsTab.Controls.Add(userToolbarPanel);
        permissionsTab.Location = new Point(4, 24);
        permissionsTab.Name = "permissionsTab";
        permissionsTab.Padding = new Padding(3);
        permissionsTab.Size = new Size(976, 587);
        permissionsTab.TabIndex = 0;
        permissionsTab.Text = "Permissões no banco";
        permissionsTab.UseVisualStyleBackColor = true;
        //
        // userTreeView
        //
        userTreeView.ContextMenuStrip = treeContextMenu;
        userTreeView.Dock = DockStyle.Fill;
        userTreeView.HideSelection = false;
        userTreeView.Location = new Point(3, 40);
        userTreeView.Name = "userTreeView";
        userTreeView.Size = new Size(970, 544);
        userTreeView.TabIndex = 1;
        userTreeView.BeforeExpand += UserTreeView_BeforeExpand;
        userTreeView.NodeMouseClick += UserTreeView_NodeMouseClick;
        userTreeView.KeyDown += UserTreeView_KeyDown;
        //
        // treeContextMenu
        //
        treeContextMenu.Name = "treeContextMenu";
        treeContextMenu.Size = new Size(61, 4);
        treeContextMenu.Opening += TreeContextMenu_Opening;
        //
        // userToolbarPanel
        //
        userToolbarPanel.AutoSize = true;
        userToolbarPanel.Controls.Add(userLabel);
        userToolbarPanel.Controls.Add(userTextBox);
        userToolbarPanel.Controls.Add(searchUserButton);
        userToolbarPanel.Controls.Add(expandAllButton);
        userToolbarPanel.Controls.Add(reportButton);
        userToolbarPanel.Controls.Add(extractScriptButton);
        userToolbarPanel.Controls.Add(cancelUserButton);
        userToolbarPanel.Dock = DockStyle.Top;
        userToolbarPanel.Location = new Point(3, 3);
        userToolbarPanel.Name = "userToolbarPanel";
        userToolbarPanel.Padding = new Padding(3, 3, 3, 3);
        userToolbarPanel.Size = new Size(970, 37);
        userToolbarPanel.TabIndex = 0;
        //
        // userLabel
        //
        userLabel.Anchor = AnchorStyles.Left;
        userLabel.AutoSize = true;
        userLabel.Location = new Point(6, 11);
        userLabel.Name = "userLabel";
        userLabel.Size = new Size(50, 15);
        userLabel.TabIndex = 0;
        userLabel.Text = "&Usuário:";
        //
        // userTextBox
        //
        userTextBox.CharacterCasing = CharacterCasing.Upper;
        userTextBox.Location = new Point(62, 6);
        userTextBox.Name = "userTextBox";
        userTextBox.Size = new Size(200, 23);
        userTextBox.TabIndex = 1;
        userTextBox.KeyDown += UserTextBox_KeyDown;
        //
        // searchUserButton
        //
        searchUserButton.AutoSize = true;
        searchUserButton.Location = new Point(268, 6);
        searchUserButton.Margin = new Padding(3, 3, 18, 3);
        searchUserButton.Name = "searchUserButton";
        searchUserButton.Size = new Size(75, 25);
        searchUserButton.TabIndex = 2;
        searchUserButton.Text = "&Pesquisar";
        searchUserButton.UseVisualStyleBackColor = true;
        searchUserButton.Click += SearchUserButton_Click;
        //
        // expandAllButton
        //
        expandAllButton.AutoSize = true;
        expandAllButton.Location = new Point(364, 6);
        expandAllButton.Name = "expandAllButton";
        expandAllButton.Size = new Size(90, 25);
        expandAllButton.TabIndex = 3;
        expandAllButton.Text = "&Expandir tudo";
        expandAllButton.UseVisualStyleBackColor = true;
        expandAllButton.Click += ExpandAllButton_Click;
        //
        // reportButton
        //
        reportButton.AutoSize = true;
        reportButton.Location = new Point(460, 6);
        reportButton.Name = "reportButton";
        reportButton.Size = new Size(100, 25);
        reportButton.TabIndex = 4;
        reportButton.Text = "Gerar &relatório";
        reportButton.UseVisualStyleBackColor = true;
        reportButton.Click += ReportButton_Click;
        //
        // extractScriptButton
        //
        extractScriptButton.AutoSize = true;
        extractScriptButton.Location = new Point(566, 6);
        extractScriptButton.Name = "extractScriptButton";
        extractScriptButton.Size = new Size(150, 25);
        extractScriptButton.TabIndex = 5;
        extractScriptButton.Text = "E&xtrair script do usuário";
        extractScriptButton.UseVisualStyleBackColor = true;
        extractScriptButton.Click += ExtractScriptButton_Click;
        //
        // cancelUserButton
        //
        cancelUserButton.AutoSize = true;
        cancelUserButton.Enabled = false;
        cancelUserButton.Location = new Point(722, 6);
        cancelUserButton.Name = "cancelUserButton";
        cancelUserButton.Size = new Size(75, 25);
        cancelUserButton.TabIndex = 6;
        cancelUserButton.Text = "Cancelar";
        cancelUserButton.UseVisualStyleBackColor = true;
        cancelUserButton.Click += CancelButton_Click;
        //
        // locateTab
        //
        locateTab.Controls.Add(reportTextBox);
        locateTab.Controls.Add(locateToolbarPanel);
        locateTab.Location = new Point(4, 24);
        locateTab.Name = "locateTab";
        locateTab.Padding = new Padding(3);
        locateTab.Size = new Size(976, 587);
        locateTab.TabIndex = 1;
        locateTab.Text = "Localizar usuário";
        locateTab.UseVisualStyleBackColor = true;
        //
        // reportTextBox
        //
        reportTextBox.Dock = DockStyle.Fill;
        reportTextBox.Font = new Font("Consolas", 9.75F);
        reportTextBox.Location = new Point(3, 40);
        reportTextBox.Multiline = true;
        reportTextBox.Name = "reportTextBox";
        reportTextBox.ReadOnly = true;
        reportTextBox.ScrollBars = ScrollBars.Both;
        reportTextBox.Size = new Size(970, 544);
        reportTextBox.TabIndex = 1;
        reportTextBox.WordWrap = false;
        reportTextBox.KeyDown += ReportTextBox_KeyDown;
        //
        // locateToolbarPanel
        //
        locateToolbarPanel.AutoSize = true;
        locateToolbarPanel.Controls.Add(locateLabel);
        locateToolbarPanel.Controls.Add(locateTextBox);
        locateToolbarPanel.Controls.Add(searchDirectoryCheckBox);
        locateToolbarPanel.Controls.Add(searchDatabaseCheckBox);
        locateToolbarPanel.Controls.Add(locateButton);
        locateToolbarPanel.Controls.Add(cancelLocateButton);
        locateToolbarPanel.Controls.Add(directoryLabel);
        locateToolbarPanel.Dock = DockStyle.Top;
        locateToolbarPanel.Location = new Point(3, 3);
        locateToolbarPanel.Name = "locateToolbarPanel";
        locateToolbarPanel.Padding = new Padding(3, 3, 3, 3);
        locateToolbarPanel.Size = new Size(970, 37);
        locateToolbarPanel.TabIndex = 0;
        //
        // locateLabel
        //
        locateLabel.Anchor = AnchorStyles.Left;
        locateLabel.AutoSize = true;
        locateLabel.Location = new Point(6, 11);
        locateLabel.Name = "locateLabel";
        locateLabel.Size = new Size(87, 15);
        locateLabel.TabIndex = 0;
        locateLabel.Text = "&Nome ou login:";
        //
        // locateTextBox
        //
        locateTextBox.Location = new Point(99, 6);
        locateTextBox.Name = "locateTextBox";
        locateTextBox.PlaceholderText = "use * ou % como curinga";
        locateTextBox.Size = new Size(240, 23);
        locateTextBox.TabIndex = 1;
        locateTextBox.KeyDown += LocateTextBox_KeyDown;
        //
        // searchDirectoryCheckBox
        //
        searchDirectoryCheckBox.Anchor = AnchorStyles.Left;
        searchDirectoryCheckBox.AutoSize = true;
        searchDirectoryCheckBox.Checked = true;
        searchDirectoryCheckBox.CheckState = CheckState.Checked;
        searchDirectoryCheckBox.Location = new Point(345, 9);
        searchDirectoryCheckBox.Name = "searchDirectoryCheckBox";
        searchDirectoryCheckBox.Size = new Size(112, 19);
        searchDirectoryCheckBox.TabIndex = 2;
        searchDirectoryCheckBox.Text = "Active Directory";
        searchDirectoryCheckBox.UseVisualStyleBackColor = true;
        //
        // searchDatabaseCheckBox
        //
        searchDatabaseCheckBox.Anchor = AnchorStyles.Left;
        searchDatabaseCheckBox.AutoSize = true;
        searchDatabaseCheckBox.Checked = true;
        searchDatabaseCheckBox.CheckState = CheckState.Checked;
        searchDatabaseCheckBox.Location = new Point(463, 9);
        searchDatabaseCheckBox.Name = "searchDatabaseCheckBox";
        searchDatabaseCheckBox.Size = new Size(108, 19);
        searchDatabaseCheckBox.TabIndex = 3;
        searchDatabaseCheckBox.Text = "Banco de dados";
        searchDatabaseCheckBox.UseVisualStyleBackColor = true;
        //
        // locateButton
        //
        locateButton.AutoSize = true;
        locateButton.Location = new Point(577, 6);
        locateButton.Name = "locateButton";
        locateButton.Size = new Size(75, 25);
        locateButton.TabIndex = 4;
        locateButton.Text = "&Localizar";
        locateButton.UseVisualStyleBackColor = true;
        locateButton.Click += LocateButton_Click;
        //
        // cancelLocateButton
        //
        cancelLocateButton.AutoSize = true;
        cancelLocateButton.Enabled = false;
        cancelLocateButton.Location = new Point(658, 6);
        cancelLocateButton.Name = "cancelLocateButton";
        cancelLocateButton.Size = new Size(75, 25);
        cancelLocateButton.TabIndex = 5;
        cancelLocateButton.Text = "Cancelar";
        cancelLocateButton.UseVisualStyleBackColor = true;
        cancelLocateButton.Click += CancelButton_Click;
        //
        // directoryLabel
        //
        directoryLabel.Anchor = AnchorStyles.Left;
        directoryLabel.AutoSize = true;
        directoryLabel.ForeColor = SystemColors.GrayText;
        directoryLabel.Location = new Point(754, 11);
        directoryLabel.Margin = new Padding(18, 0, 3, 0);
        directoryLabel.Name = "directoryLabel";
        directoryLabel.Size = new Size(180, 15);
        directoryLabel.TabIndex = 6;
        directoryLabel.Text = "Active Directory: não conectado";
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 639);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(984, 22);
        statusStrip.TabIndex = 2;
        //
        // statusLabel
        //
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(747, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Pronto";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // progressBar
        //
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(220, 16);
        //
        // UserDetailsForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(984, 661);
        Controls.Add(tabControl);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 450);
        Name = "UserDetailsForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Detalhes do usuário";
        Load += UserDetailsForm_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        tabControl.ResumeLayout(false);
        permissionsTab.ResumeLayout(false);
        permissionsTab.PerformLayout();
        userToolbarPanel.ResumeLayout(false);
        userToolbarPanel.PerformLayout();
        locateTab.ResumeLayout(false);
        locateTab.PerformLayout();
        locateToolbarPanel.ResumeLayout(false);
        locateToolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem connectMenuItem;
    private ToolStripMenuItem directoryConnectMenuItem;
    private ToolStripMenuItem serverInfoMenuItem;
    private ToolStripSeparator fileSeparator;
    private ToolStripMenuItem exitMenuItem;
    private TabControl tabControl;
    private TabPage permissionsTab;
    private TreeView userTreeView;
    private ContextMenuStrip treeContextMenu;
    private FlowLayoutPanel userToolbarPanel;
    private Label userLabel;
    private TextBox userTextBox;
    private Button searchUserButton;
    private Button expandAllButton;
    private Button reportButton;
    private Button extractScriptButton;
    private Button cancelUserButton;
    private TabPage locateTab;
    private TextBox reportTextBox;
    private FlowLayoutPanel locateToolbarPanel;
    private Label locateLabel;
    private TextBox locateTextBox;
    private CheckBox searchDirectoryCheckBox;
    private CheckBox searchDatabaseCheckBox;
    private Button locateButton;
    private Button cancelLocateButton;
    private Label directoryLabel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
