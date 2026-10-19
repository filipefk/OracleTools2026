namespace OracleTools2026.UI;

partial class DatabaseLinksForm
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
        testAllButton = new Button();
        testSelectedButton = new Button();
        cancelButton = new Button();
        listCountLabel = new Label();
        linksListView = new ListView();
        createdColumn = new ColumnHeader();
        ownerColumn = new ColumnHeader();
        nameColumn = new ColumnHeader();
        testColumn = new ColumnHeader();
        userNameColumn = new ColumnHeader();
        hostColumn = new ColumnHeader();
        linkContextMenu = new ContextMenuStrip(components);
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
        menuStrip.Size = new Size(1084, 24);
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
        toolbarPanel.Controls.Add(testAllButton);
        toolbarPanel.Controls.Add(testSelectedButton);
        toolbarPanel.Controls.Add(cancelButton);
        toolbarPanel.Controls.Add(listCountLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 24);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(6, 6, 6, 0);
        toolbarPanel.Size = new Size(1084, 41);
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
        // testAllButton
        //
        testAllButton.Location = new Point(115, 9);
        testAllButton.Name = "testAllButton";
        testAllButton.Size = new Size(110, 29);
        testAllButton.TabIndex = 1;
        testAllButton.Text = "Testar &todos";
        testAllButton.UseVisualStyleBackColor = true;
        testAllButton.Click += TestAllButton_Click;
        //
        // testSelectedButton
        //
        testSelectedButton.Location = new Point(231, 9);
        testSelectedButton.Name = "testSelectedButton";
        testSelectedButton.Size = new Size(130, 29);
        testSelectedButton.TabIndex = 2;
        testSelectedButton.Text = "Testar &selecionados";
        testSelectedButton.UseVisualStyleBackColor = true;
        testSelectedButton.Click += TestSelectedButton_Click;
        //
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(367, 9);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(100, 29);
        cancelButton.TabIndex = 3;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        //
        // listCountLabel
        //
        listCountLabel.Anchor = AnchorStyles.Left;
        listCountLabel.AutoSize = true;
        listCountLabel.Location = new Point(485, 16);
        listCountLabel.Margin = new Padding(15, 0, 3, 0);
        listCountLabel.Name = "listCountLabel";
        listCountLabel.Size = new Size(110, 15);
        listCountLabel.TabIndex = 4;
        listCountLabel.Text = "0 DBLINKs listados";
        //
        // linksListView
        //
        linksListView.Columns.AddRange(new ColumnHeader[] { createdColumn, ownerColumn, nameColumn, testColumn, userNameColumn, hostColumn });
        linksListView.ContextMenuStrip = linkContextMenu;
        linksListView.Dock = DockStyle.Fill;
        linksListView.FullRowSelect = true;
        linksListView.GridLines = true;
        linksListView.Location = new Point(0, 65);
        linksListView.Name = "linksListView";
        linksListView.Size = new Size(1084, 474);
        linksListView.TabIndex = 2;
        linksListView.UseCompatibleStateImageBehavior = false;
        linksListView.View = View.Details;
        linksListView.ColumnClick += LinksListView_ColumnClick;
        linksListView.KeyDown += LinksListView_KeyDown;
        //
        // createdColumn
        //
        createdColumn.Text = "CREATED";
        createdColumn.Width = 120;
        //
        // ownerColumn
        //
        ownerColumn.Text = "OWNER";
        ownerColumn.Width = 100;
        //
        // nameColumn
        //
        nameColumn.Text = "DB_LINK";
        nameColumn.Width = 200;
        //
        // testColumn
        //
        testColumn.Text = "TESTE";
        testColumn.Width = 250;
        //
        // userNameColumn
        //
        userNameColumn.Text = "USERNAME";
        userNameColumn.Width = 100;
        //
        // hostColumn
        //
        hostColumn.Text = "HOST";
        hostColumn.Width = 250;
        //
        // linkContextMenu
        //
        linkContextMenu.Name = "linkContextMenu";
        linkContextMenu.Size = new Size(61, 4);
        linkContextMenu.Opening += LinkContextMenu_Opening;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 539);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1084, 22);
        statusStrip.TabIndex = 3;
        //
        // statusLabel
        //
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(847, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Pronto";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // progressBar
        //
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(220, 16);
        //
        // DatabaseLinksForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1084, 561);
        Controls.Add(linksListView);
        Controls.Add(toolbarPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 400);
        Name = "DatabaseLinksForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DBLINKs";
        Load += DatabaseLinksForm_Load;
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
    private Button testAllButton;
    private Button testSelectedButton;
    private Button cancelButton;
    private Label listCountLabel;
    private ListView linksListView;
    private ColumnHeader createdColumn;
    private ColumnHeader ownerColumn;
    private ColumnHeader nameColumn;
    private ColumnHeader testColumn;
    private ColumnHeader userNameColumn;
    private ColumnHeader hostColumn;
    private ContextMenuStrip linkContextMenu;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
