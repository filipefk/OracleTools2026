namespace OracleTools2026.UI;

partial class MainForm
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
        newInstanceMenuItem = new ToolStripMenuItem();
        connectionSeparator = new ToolStripSeparator();
        settingsMenuItem = new ToolStripMenuItem();
        fileSeparator = new ToolStripSeparator();
        exitMenuItem = new ToolStripMenuItem();
        headerPanel = new Panel();
        subtitleLabel = new Label();
        titleLabel = new Label();
        toolsPanel = new FlowLayoutPanel();
        statusStrip = new StatusStrip();
        hintLabel = new ToolStripStatusLabel();
        connectionLabel = new ToolStripStatusLabel();
        versionLabel = new ToolStripStatusLabel();
        toolTip = new ToolTip(components);
        menuStrip.SuspendLayout();
        headerPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(764, 24);
        menuStrip.TabIndex = 0;
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { connectMenuItem, newInstanceMenuItem, connectionSeparator, settingsMenuItem, fileSeparator, exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(61, 20);
        fileMenu.Text = "&Arquivo";
        //
        // connectMenuItem
        //
        connectMenuItem.Name = "connectMenuItem";
        connectMenuItem.ShortcutKeys = Keys.Control | Keys.L;
        connectMenuItem.Size = new Size(200, 22);
        connectMenuItem.Text = "&Conectar...";
        connectMenuItem.Click += ConnectMenuItem_Click;
        //
        // newInstanceMenuItem
        //
        newInstanceMenuItem.Name = "newInstanceMenuItem";
        newInstanceMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newInstanceMenuItem.Size = new Size(200, 22);
        newInstanceMenuItem.Text = "&Nova instância";
        newInstanceMenuItem.Click += NewInstanceMenuItem_Click;
        //
        // connectionSeparator
        //
        connectionSeparator.Name = "connectionSeparator";
        connectionSeparator.Size = new Size(197, 6);
        //
        // settingsMenuItem
        //
        settingsMenuItem.Name = "settingsMenuItem";
        settingsMenuItem.Size = new Size(200, 22);
        settingsMenuItem.Text = "&Parâmetros...";
        settingsMenuItem.Click += SettingsMenuItem_Click;
        // 
        // fileSeparator
        // 
        fileSeparator.Name = "fileSeparator";
        fileSeparator.Size = new Size(197, 6);
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(200, 22);
        exitMenuItem.Text = "Sai&r";
        exitMenuItem.Click += ExitMenuItem_Click;
        // 
        // headerPanel
        // 
        headerPanel.AutoSize = true;
        headerPanel.Controls.Add(subtitleLabel);
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 24);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(16, 12, 16, 8);
        headerPanel.Size = new Size(764, 77);
        headerPanel.TabIndex = 1;
        // 
        // subtitleLabel
        // 
        subtitleLabel.AutoSize = true;
        subtitleLabel.Dock = DockStyle.Top;
        subtitleLabel.ForeColor = SystemColors.GrayText;
        subtitleLabel.Location = new Point(16, 42);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Padding = new Padding(0, 4, 0, 8);
        subtitleLabel.Size = new Size(640, 27);
        subtitleLabel.TabIndex = 1;
        subtitleLabel.Text = "Escolha uma ferramenta. Todas usam a mesma conexão; para usar outra ao mesmo tempo, abra uma nova instância.";
        // 
        // titleLabel
        // 
        titleLabel.AutoSize = true;
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
        titleLabel.Location = new Point(16, 12);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(134, 30);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Oracle Tools";
        // 
        // toolsPanel
        // 
        toolsPanel.AutoScroll = true;
        toolsPanel.Dock = DockStyle.Fill;
        toolsPanel.Location = new Point(0, 101);
        toolsPanel.Name = "toolsPanel";
        toolsPanel.Padding = new Padding(10, 0, 10, 10);
        toolsPanel.Size = new Size(764, 338);
        toolsPanel.TabIndex = 2;
        toolsPanel.ClientSizeChanged += ToolsPanel_ClientSizeChanged;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { hintLabel, connectionLabel, versionLabel });
        statusStrip.Location = new Point(0, 439);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(764, 22);
        statusStrip.TabIndex = 3;
        // 
        // hintLabel
        // 
        hintLabel.ForeColor = SystemColors.GrayText;
        hintLabel.Name = "hintLabel";
        hintLabel.Size = new Size(583, 17);
        hintLabel.Spring = true;
        hintLabel.Text = "Use Tab e Enter para abrir uma ferramenta pelo teclado.";
        hintLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // connectionLabel
        //
        connectionLabel.Name = "connectionLabel";
        connectionLabel.Size = new Size(98, 17);
        connectionLabel.Text = "Não conectado";
        //
        // versionLabel
        // 
        versionLabel.ForeColor = SystemColors.GrayText;
        versionLabel.Name = "versionLabel";
        versionLabel.Size = new Size(68, 17);
        versionLabel.Text = "Versão 1.0.0";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.Window;
        ClientSize = new Size(764, 461);
        Controls.Add(toolsPanel);
        Controls.Add(headerPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(420, 360);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Oracle Tools";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem connectMenuItem;
    private ToolStripMenuItem newInstanceMenuItem;
    private ToolStripSeparator connectionSeparator;
    private ToolStripMenuItem settingsMenuItem;
    private ToolStripSeparator fileSeparator;
    private ToolStripMenuItem exitMenuItem;
    private Panel headerPanel;
    private Label subtitleLabel;
    private Label titleLabel;
    private FlowLayoutPanel toolsPanel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel hintLabel;
    private ToolStripStatusLabel connectionLabel;
    private ToolStripStatusLabel versionLabel;
    private ToolTip toolTip;
}
