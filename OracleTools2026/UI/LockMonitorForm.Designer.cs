namespace OracleTools2026.UI;

partial class LockMonitorForm
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
        script1Button = new Button();
        scriptContextMenu = new ContextMenuStrip(components);
        copyScriptMenuItem = new ToolStripMenuItem();
        script2Button = new Button();
        stopButton = new Button();
        intervalLabel = new Label();
        intervalNumeric = new NumericUpDown();
        notifyCheckBox = new CheckBox();
        stateLabel = new Label();
        locksListView = new ListView();
        listContextMenu = new ContextMenuStrip(components);
        copySelectedMenuItem = new ToolStripMenuItem();
        copyAllMenuItem = new ToolStripMenuItem();
        listSeparator = new ToolStripSeparator();
        listServerInfoMenuItem = new ToolStripMenuItem();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        menuStrip.SuspendLayout();
        toolbarPanel.SuspendLayout();
        scriptContextMenu.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)intervalNumeric).BeginInit();
        listContextMenu.SuspendLayout();
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
        toolbarPanel.Controls.Add(script1Button);
        toolbarPanel.Controls.Add(script2Button);
        toolbarPanel.Controls.Add(stopButton);
        toolbarPanel.Controls.Add(intervalLabel);
        toolbarPanel.Controls.Add(intervalNumeric);
        toolbarPanel.Controls.Add(notifyCheckBox);
        toolbarPanel.Controls.Add(stateLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 24);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(6, 6, 6, 0);
        toolbarPanel.Size = new Size(984, 41);
        toolbarPanel.TabIndex = 1;
        //
        // script1Button
        //
        script1Button.ContextMenuStrip = scriptContextMenu;
        script1Button.Location = new Point(9, 9);
        script1Button.Name = "script1Button";
        script1Button.Size = new Size(100, 29);
        script1Button.TabIndex = 0;
        script1Button.Text = "Script &1";
        script1Button.UseVisualStyleBackColor = true;
        script1Button.Click += Script1Button_Click;
        //
        // scriptContextMenu
        //
        scriptContextMenu.Items.AddRange(new ToolStripItem[] { copyScriptMenuItem });
        scriptContextMenu.Name = "scriptContextMenu";
        scriptContextMenu.Size = new Size(279, 26);
        //
        // copyScriptMenuItem
        //
        copyScriptMenuItem.Name = "copyScriptMenuItem";
        copyScriptMenuItem.Size = new Size(278, 22);
        copyScriptMenuItem.Text = "Copiar script para a área de transferência";
        copyScriptMenuItem.Click += CopyScriptMenuItem_Click;
        //
        // script2Button
        //
        script2Button.ContextMenuStrip = scriptContextMenu;
        script2Button.Location = new Point(115, 9);
        script2Button.Name = "script2Button";
        script2Button.Size = new Size(100, 29);
        script2Button.TabIndex = 1;
        script2Button.Text = "Script &2";
        script2Button.UseVisualStyleBackColor = true;
        script2Button.Click += Script2Button_Click;
        //
        // stopButton
        //
        stopButton.Enabled = false;
        stopButton.Location = new Point(221, 9);
        stopButton.Name = "stopButton";
        stopButton.Size = new Size(100, 29);
        stopButton.TabIndex = 2;
        stopButton.Text = "&Parar";
        stopButton.UseVisualStyleBackColor = true;
        stopButton.Click += StopButton_Click;
        //
        // intervalLabel
        //
        intervalLabel.Anchor = AnchorStyles.Left;
        intervalLabel.AutoSize = true;
        intervalLabel.Location = new Point(339, 16);
        intervalLabel.Margin = new Padding(15, 0, 3, 0);
        intervalLabel.Name = "intervalLabel";
        intervalLabel.Size = new Size(122, 15);
        intervalLabel.TabIndex = 3;
        intervalLabel.Text = "&Intervalo (segundos):";
        //
        // intervalNumeric
        //
        intervalNumeric.Anchor = AnchorStyles.Left;
        intervalNumeric.Location = new Point(467, 12);
        intervalNumeric.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        intervalNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        intervalNumeric.Name = "intervalNumeric";
        intervalNumeric.Size = new Size(60, 23);
        intervalNumeric.TabIndex = 4;
        intervalNumeric.Value = new decimal(new int[] { 5, 0, 0, 0 });
        //
        // notifyCheckBox
        //
        notifyCheckBox.Anchor = AnchorStyles.Left;
        notifyCheckBox.AutoSize = true;
        notifyCheckBox.Checked = true;
        notifyCheckBox.CheckState = CheckState.Checked;
        notifyCheckBox.Location = new Point(545, 14);
        notifyCheckBox.Margin = new Padding(15, 3, 3, 3);
        notifyCheckBox.Name = "notifyCheckBox";
        notifyCheckBox.Size = new Size(165, 19);
        notifyCheckBox.TabIndex = 5;
        notifyCheckBox.Text = "&Avisar caso encontre lock";
        notifyCheckBox.UseVisualStyleBackColor = true;
        //
        // stateLabel
        //
        stateLabel.Anchor = AnchorStyles.Left;
        stateLabel.AutoSize = true;
        stateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        stateLabel.ForeColor = SystemColors.GrayText;
        stateLabel.Location = new Point(728, 16);
        stateLabel.Margin = new Padding(15, 0, 3, 0);
        stateLabel.Name = "stateLabel";
        stateLabel.Size = new Size(45, 15);
        stateLabel.TabIndex = 6;
        stateLabel.Text = "Parado";
        //
        // locksListView
        //
        locksListView.ContextMenuStrip = listContextMenu;
        locksListView.Dock = DockStyle.Fill;
        locksListView.FullRowSelect = true;
        locksListView.GridLines = true;
        locksListView.Location = new Point(0, 65);
        locksListView.Name = "locksListView";
        locksListView.Size = new Size(984, 474);
        locksListView.TabIndex = 2;
        locksListView.UseCompatibleStateImageBehavior = false;
        locksListView.View = View.Details;
        locksListView.ColumnClick += LocksListView_ColumnClick;
        locksListView.KeyDown += LocksListView_KeyDown;
        //
        // listContextMenu
        //
        listContextMenu.Items.AddRange(new ToolStripItem[] { copySelectedMenuItem, copyAllMenuItem, listSeparator, listServerInfoMenuItem });
        listContextMenu.Name = "listContextMenu";
        listContextMenu.Size = new Size(240, 76);
        listContextMenu.Opening += ListContextMenu_Opening;
        //
        // copySelectedMenuItem
        //
        copySelectedMenuItem.Name = "copySelectedMenuItem";
        copySelectedMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
        copySelectedMenuItem.Size = new Size(239, 22);
        copySelectedMenuItem.Text = "Copiar linhas selecionadas";
        copySelectedMenuItem.Click += CopySelectedMenuItem_Click;
        //
        // copyAllMenuItem
        //
        copyAllMenuItem.Name = "copyAllMenuItem";
        copyAllMenuItem.Size = new Size(239, 22);
        copyAllMenuItem.Text = "Copiar toda a lista";
        copyAllMenuItem.Click += CopyAllMenuItem_Click;
        //
        // listSeparator
        //
        listSeparator.Name = "listSeparator";
        listSeparator.Size = new Size(236, 6);
        //
        // listServerInfoMenuItem
        //
        listServerInfoMenuItem.Name = "listServerInfoMenuItem";
        listServerInfoMenuItem.Size = new Size(239, 22);
        listServerInfoMenuItem.Text = "Informações do banco de dados";
        listServerInfoMenuItem.Click += ListServerInfoMenuItem_Click;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 539);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(984, 22);
        statusStrip.TabIndex = 3;
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
        // LockMonitorForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(984, 561);
        Controls.Add(locksListView);
        Controls.Add(toolbarPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 400);
        Name = "LockMonitorForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Procura Lock no banco";
        FormClosing += LockMonitorForm_FormClosing;
        Load += LockMonitorForm_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        scriptContextMenu.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)intervalNumeric).EndInit();
        listContextMenu.ResumeLayout(false);
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
    private Button script1Button;
    private ContextMenuStrip scriptContextMenu;
    private ToolStripMenuItem copyScriptMenuItem;
    private Button script2Button;
    private Button stopButton;
    private Label intervalLabel;
    private NumericUpDown intervalNumeric;
    private CheckBox notifyCheckBox;
    private Label stateLabel;
    private ListView locksListView;
    private ContextMenuStrip listContextMenu;
    private ToolStripMenuItem copySelectedMenuItem;
    private ToolStripMenuItem copyAllMenuItem;
    private ToolStripSeparator listSeparator;
    private ToolStripMenuItem listServerInfoMenuItem;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
