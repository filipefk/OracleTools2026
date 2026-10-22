namespace OracleTools2026.UI;

partial class SessionsForm
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
        serverInfoMenuItem = new ToolStripMenuItem();
        fileSeparator = new ToolStripSeparator();
        exitMenuItem = new ToolStripMenuItem();
        toolbarPanel = new FlowLayoutPanel();
        refreshButton = new Button();
        killButton = new Button();
        cancelButton = new Button();
        selectAllCheckBox = new CheckBox();
        listCountLabel = new Label();
        legendLabel = new Label();
        sessionsListView = new ListView();
        instanceColumn = new ColumnHeader();
        sidColumn = new ColumnHeader();
        serialColumn = new ColumnHeader();
        userNameColumn = new ColumnHeader();
        userRealNameColumn = new ColumnHeader();
        osUserColumn = new ColumnHeader();
        osUserRealNameColumn = new ColumnHeader();
        machineColumn = new ColumnHeader();
        statusColumn = new ColumnHeader();
        programColumn = new ColumnHeader();
        typeColumn = new ColumnHeader();
        moduleColumn = new ColumnHeader();
        logonTimeColumn = new ColumnHeader();
        stateColumn = new ColumnHeader();
        sessionContextMenu = new ContextMenuStrip(components);
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        menuStrip.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // menuStrip
        //
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1184, 24);
        menuStrip.TabIndex = 0;
        //
        // fileMenu
        //
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { connectMenuItem, serverInfoMenuItem, fileSeparator, exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(61, 20);
        fileMenu.Text = "&Arquivo";
        //
        // connectMenuItem
        //
        connectMenuItem.Name = "connectMenuItem";
        connectMenuItem.ShortcutKeys = Keys.Control | Keys.L;
        connectMenuItem.Size = new Size(193, 22);
        connectMenuItem.Text = "&Conectar...";
        connectMenuItem.Click += ConnectMenuItem_Click;
        //
        // serverInfoMenuItem
        //
        serverInfoMenuItem.Name = "serverInfoMenuItem";
        serverInfoMenuItem.Size = new Size(193, 22);
        serverInfoMenuItem.Text = "&Informações do banco";
        serverInfoMenuItem.Click += ServerInfoMenuItem_Click;
        //
        // fileSeparator
        //
        fileSeparator.Name = "fileSeparator";
        fileSeparator.Size = new Size(190, 6);
        //
        // exitMenuItem
        //
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(193, 22);
        exitMenuItem.Text = "Sai&r";
        exitMenuItem.Click += ExitMenuItem_Click;
        //
        // toolbarPanel
        //
        toolbarPanel.AutoSize = true;
        toolbarPanel.Controls.Add(refreshButton);
        toolbarPanel.Controls.Add(killButton);
        toolbarPanel.Controls.Add(cancelButton);
        toolbarPanel.Controls.Add(selectAllCheckBox);
        toolbarPanel.Controls.Add(listCountLabel);
        toolbarPanel.Controls.Add(legendLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 24);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(6, 6, 6, 0);
        toolbarPanel.Size = new Size(1184, 41);
        toolbarPanel.TabIndex = 1;
        //
        // refreshButton
        //
        refreshButton.Location = new Point(9, 9);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(100, 29);
        refreshButton.TabIndex = 0;
        refreshButton.Text = "&Atualizar";
        refreshButton.UseVisualStyleBackColor = true;
        refreshButton.Click += RefreshButton_Click;
        //
        // killButton
        //
        killButton.Location = new Point(115, 9);
        killButton.Name = "killButton";
        killButton.Size = new Size(120, 29);
        killButton.TabIndex = 1;
        killButton.Text = "&Matar sessões";
        killButton.UseVisualStyleBackColor = true;
        killButton.Click += KillButton_Click;
        //
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(241, 9);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(100, 29);
        cancelButton.TabIndex = 2;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        //
        // selectAllCheckBox
        //
        selectAllCheckBox.Anchor = AnchorStyles.Left;
        selectAllCheckBox.AutoSize = true;
        selectAllCheckBox.Location = new Point(359, 14);
        selectAllCheckBox.Margin = new Padding(15, 3, 3, 3);
        selectAllCheckBox.Name = "selectAllCheckBox";
        selectAllCheckBox.Size = new Size(97, 19);
        selectAllCheckBox.TabIndex = 3;
        selectAllCheckBox.Text = "Marcar &todas";
        selectAllCheckBox.UseVisualStyleBackColor = true;
        selectAllCheckBox.CheckStateChanged += SelectAllCheckBox_CheckStateChanged;
        //
        // listCountLabel
        //
        listCountLabel.Anchor = AnchorStyles.Left;
        listCountLabel.AutoSize = true;
        listCountLabel.Location = new Point(474, 16);
        listCountLabel.Margin = new Padding(15, 0, 3, 0);
        listCountLabel.Name = "listCountLabel";
        listCountLabel.Size = new Size(176, 15);
        listCountLabel.TabIndex = 4;
        listCountLabel.Text = "0 sessões listadas - 0 marcadas";
        //
        // legendLabel
        //
        legendLabel.Anchor = AnchorStyles.Left;
        legendLabel.AutoSize = true;
        legendLabel.ForeColor = SystemColors.GrayText;
        legendLabel.Location = new Point(668, 16);
        legendLabel.Margin = new Padding(15, 0, 3, 0);
        legendLabel.Name = "legendLabel";
        legendLabel.Size = new Size(470, 15);
        legendLabel.TabIndex = 5;
        legendLabel.Text = "Cinza: usuário do banco e do SO de pessoas diferentes";
        //
        // sessionsListView
        //
        sessionsListView.CheckBoxes = true;
        sessionsListView.Columns.AddRange(new ColumnHeader[] { instanceColumn, sidColumn, serialColumn, userNameColumn, userRealNameColumn, osUserColumn, osUserRealNameColumn, machineColumn, statusColumn, programColumn, typeColumn, moduleColumn, logonTimeColumn, stateColumn });
        sessionsListView.ContextMenuStrip = sessionContextMenu;
        sessionsListView.Dock = DockStyle.Fill;
        sessionsListView.FullRowSelect = true;
        sessionsListView.GridLines = true;
        sessionsListView.Location = new Point(0, 65);
        sessionsListView.Name = "sessionsListView";
        sessionsListView.Size = new Size(1184, 574);
        sessionsListView.TabIndex = 2;
        sessionsListView.UseCompatibleStateImageBehavior = false;
        sessionsListView.View = View.Details;
        sessionsListView.ColumnClick += SessionsListView_ColumnClick;
        sessionsListView.ItemChecked += SessionsListView_ItemChecked;
        sessionsListView.KeyDown += SessionsListView_KeyDown;
        //
        // instanceColumn
        //
        instanceColumn.Text = "INST_ID";
        instanceColumn.Width = 70;
        //
        // sidColumn
        //
        sidColumn.Text = "SID";
        //
        // serialColumn
        //
        serialColumn.Text = "SERIAL#";
        serialColumn.Width = 70;
        //
        // userNameColumn
        //
        userNameColumn.Text = "USERNAME";
        userNameColumn.Width = 100;
        //
        // userRealNameColumn
        //
        userRealNameColumn.Text = "NOME_USERNAME";
        userRealNameColumn.Width = 140;
        //
        // osUserColumn
        //
        osUserColumn.Text = "OSUSER";
        osUserColumn.Width = 100;
        //
        // osUserRealNameColumn
        //
        osUserRealNameColumn.Text = "NOME_OSUSER";
        osUserRealNameColumn.Width = 140;
        //
        // machineColumn
        //
        machineColumn.Text = "MACHINE";
        machineColumn.Width = 120;
        //
        // statusColumn
        //
        statusColumn.Text = "STATUS";
        statusColumn.Width = 70;
        //
        // programColumn
        //
        programColumn.Text = "PROGRAM";
        programColumn.Width = 120;
        //
        // typeColumn
        //
        typeColumn.Text = "TYPE";
        typeColumn.Width = 80;
        //
        // moduleColumn
        //
        moduleColumn.Text = "MODULE";
        moduleColumn.Width = 120;
        //
        // logonTimeColumn
        //
        logonTimeColumn.Text = "LOGON_TIME";
        logonTimeColumn.Width = 120;
        //
        // stateColumn
        //
        stateColumn.Text = "STATE";
        stateColumn.Width = 100;
        //
        // sessionContextMenu
        //
        sessionContextMenu.Name = "sessionContextMenu";
        sessionContextMenu.Size = new Size(61, 4);
        sessionContextMenu.Opening += SessionContextMenu_Opening;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 639);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1184, 22);
        statusStrip.TabIndex = 3;
        //
        // statusLabel
        //
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(947, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Pronto";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // progressBar
        //
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(220, 16);
        //
        // SessionsForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 661);
        Controls.Add(sessionsListView);
        Controls.Add(toolbarPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 400);
        Name = "SessionsForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sessões";
        Load += SessionsForm_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem connectMenuItem;
    private ToolStripMenuItem serverInfoMenuItem;
    private ToolStripSeparator fileSeparator;
    private ToolStripMenuItem exitMenuItem;
    private FlowLayoutPanel toolbarPanel;
    private Button refreshButton;
    private Button killButton;
    private Button cancelButton;
    private CheckBox selectAllCheckBox;
    private Label listCountLabel;
    private Label legendLabel;
    private ListView sessionsListView;
    private ColumnHeader instanceColumn;
    private ColumnHeader sidColumn;
    private ColumnHeader serialColumn;
    private ColumnHeader userNameColumn;
    private ColumnHeader userRealNameColumn;
    private ColumnHeader osUserColumn;
    private ColumnHeader osUserRealNameColumn;
    private ColumnHeader machineColumn;
    private ColumnHeader statusColumn;
    private ColumnHeader programColumn;
    private ColumnHeader typeColumn;
    private ColumnHeader moduleColumn;
    private ColumnHeader logonTimeColumn;
    private ColumnHeader stateColumn;
    private ContextMenuStrip sessionContextMenu;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
