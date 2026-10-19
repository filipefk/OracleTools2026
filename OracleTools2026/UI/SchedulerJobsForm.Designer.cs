namespace OracleTools2026.UI;

partial class SchedulerJobsForm
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
        cancelButton = new Button();
        listCountLabel = new Label();
        overdueLegendLabel = new Label();
        longRunningLegendLabel = new Label();
        disabledLegendLabel = new Label();
        jobsListView = new ListView();
        createdColumn = new ColumnHeader();
        lastDdlColumn = new ColumnHeader();
        ownerColumn = new ColumnHeader();
        nameColumn = new ColumnHeader();
        actionColumn = new ColumnHeader();
        enabledColumn = new ColumnHeader();
        jobTypeColumn = new ColumnHeader();
        programOwnerColumn = new ColumnHeader();
        programNameColumn = new ColumnHeader();
        scheduleOwnerColumn = new ColumnHeader();
        scheduleNameColumn = new ColumnHeader();
        scheduleTypeColumn = new ColumnHeader();
        repeatIntervalColumn = new ColumnHeader();
        lastStartColumn = new ColumnHeader();
        lastDurationColumn = new ColumnHeader();
        nextRunColumn = new ColumnHeader();
        runCountColumn = new ColumnHeader();
        failureCountColumn = new ColumnHeader();
        commentsColumn = new ColumnHeader();
        stateColumn = new ColumnHeader();
        healthColumn = new ColumnHeader();
        jobContextMenu = new ContextMenuStrip(components);
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
        toolbarPanel.Controls.Add(cancelButton);
        toolbarPanel.Controls.Add(listCountLabel);
        toolbarPanel.Controls.Add(overdueLegendLabel);
        toolbarPanel.Controls.Add(longRunningLegendLabel);
        toolbarPanel.Controls.Add(disabledLegendLabel);
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
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(115, 9);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(100, 29);
        cancelButton.TabIndex = 1;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        //
        // listCountLabel
        //
        listCountLabel.Anchor = AnchorStyles.Left;
        listCountLabel.AutoSize = true;
        listCountLabel.Location = new Point(233, 16);
        listCountLabel.Margin = new Padding(15, 0, 3, 0);
        listCountLabel.Name = "listCountLabel";
        listCountLabel.Size = new Size(90, 15);
        listCountLabel.TabIndex = 2;
        listCountLabel.Text = "0 jobs listados";
        //
        // overdueLegendLabel
        //
        overdueLegendLabel.Anchor = AnchorStyles.Left;
        overdueLegendLabel.AutoSize = true;
        overdueLegendLabel.ForeColor = Color.Red;
        overdueLegendLabel.Location = new Point(341, 16);
        overdueLegendLabel.Margin = new Padding(15, 0, 3, 0);
        overdueLegendLabel.Name = "overdueLegendLabel";
        overdueLegendLabel.Size = new Size(200, 15);
        overdueLegendLabel.TabIndex = 3;
        overdueLegendLabel.Text = "■ Atrasado (possivelmente travado)";
        //
        // longRunningLegendLabel
        //
        longRunningLegendLabel.Anchor = AnchorStyles.Left;
        longRunningLegendLabel.AutoSize = true;
        longRunningLegendLabel.ForeColor = Color.DarkOrange;
        longRunningLegendLabel.Location = new Point(559, 16);
        longRunningLegendLabel.Margin = new Padding(15, 0, 3, 0);
        longRunningLegendLabel.Name = "longRunningLegendLabel";
        longRunningLegendLabel.Size = new Size(165, 15);
        longRunningLegendLabel.TabIndex = 4;
        longRunningLegendLabel.Text = "■ Rodando há mais de 1 hora";
        //
        // disabledLegendLabel
        //
        disabledLegendLabel.Anchor = AnchorStyles.Left;
        disabledLegendLabel.AutoSize = true;
        disabledLegendLabel.ForeColor = Color.Silver;
        disabledLegendLabel.Location = new Point(742, 16);
        disabledLegendLabel.Margin = new Padding(15, 0, 3, 0);
        disabledLegendLabel.Name = "disabledLegendLabel";
        disabledLegendLabel.Size = new Size(90, 15);
        disabledLegendLabel.TabIndex = 5;
        disabledLegendLabel.Text = "■ Desabilitado";
        //
        // jobsListView
        //
        jobsListView.Columns.AddRange(new ColumnHeader[] { createdColumn, lastDdlColumn, ownerColumn, nameColumn, actionColumn, enabledColumn, jobTypeColumn, programOwnerColumn, programNameColumn, scheduleOwnerColumn, scheduleNameColumn, scheduleTypeColumn, repeatIntervalColumn, lastStartColumn, lastDurationColumn, nextRunColumn, runCountColumn, failureCountColumn, commentsColumn, stateColumn, healthColumn });
        jobsListView.ContextMenuStrip = jobContextMenu;
        jobsListView.Dock = DockStyle.Fill;
        jobsListView.FullRowSelect = true;
        jobsListView.GridLines = true;
        jobsListView.Location = new Point(0, 65);
        jobsListView.Name = "jobsListView";
        jobsListView.Size = new Size(1184, 574);
        jobsListView.TabIndex = 2;
        jobsListView.UseCompatibleStateImageBehavior = false;
        jobsListView.View = View.Details;
        jobsListView.ColumnClick += JobsListView_ColumnClick;
        jobsListView.DoubleClick += JobsListView_DoubleClick;
        jobsListView.KeyDown += JobsListView_KeyDown;
        //
        // createdColumn
        //
        createdColumn.Text = "CRIACAO";
        createdColumn.Width = 120;
        //
        // lastDdlColumn
        //
        lastDdlColumn.Text = "ULTIMA_ALTERACAO";
        lastDdlColumn.Width = 120;
        //
        // ownerColumn
        //
        ownerColumn.Text = "OWNER";
        ownerColumn.Width = 100;
        //
        // nameColumn
        //
        nameColumn.Text = "JOB_NAME";
        nameColumn.Width = 160;
        //
        // actionColumn
        //
        actionColumn.Text = "JOB_ACTION";
        actionColumn.Width = 250;
        //
        // enabledColumn
        //
        enabledColumn.Text = "ENABLED";
        enabledColumn.Width = 65;
        //
        // jobTypeColumn
        //
        jobTypeColumn.Text = "JOB_TYPE";
        jobTypeColumn.Width = 100;
        //
        // programOwnerColumn
        //
        programOwnerColumn.Text = "PROGRAM_OWNER";
        programOwnerColumn.Width = 100;
        //
        // programNameColumn
        //
        programNameColumn.Text = "PROGRAM_NAME";
        programNameColumn.Width = 120;
        //
        // scheduleOwnerColumn
        //
        scheduleOwnerColumn.Text = "SCHEDULE_OWNER";
        scheduleOwnerColumn.Width = 100;
        //
        // scheduleNameColumn
        //
        scheduleNameColumn.Text = "SCHEDULE_NAME";
        scheduleNameColumn.Width = 120;
        //
        // scheduleTypeColumn
        //
        scheduleTypeColumn.Text = "SCHEDULE_TYPE";
        scheduleTypeColumn.Width = 100;
        //
        // repeatIntervalColumn
        //
        repeatIntervalColumn.Text = "REPEAT_INTERVAL";
        repeatIntervalColumn.Width = 200;
        //
        // lastStartColumn
        //
        lastStartColumn.Text = "LAST_START_DATE";
        lastStartColumn.Width = 120;
        //
        // lastDurationColumn
        //
        lastDurationColumn.Text = "LAST_RUN_DURATION";
        lastDurationColumn.Width = 110;
        //
        // nextRunColumn
        //
        nextRunColumn.Text = "NEXT_RUN_DATE";
        nextRunColumn.Width = 120;
        //
        // runCountColumn
        //
        runCountColumn.Text = "RUN_COUNT";
        runCountColumn.Width = 75;
        //
        // failureCountColumn
        //
        failureCountColumn.Text = "FAILURE_COUNT";
        failureCountColumn.Width = 90;
        //
        // commentsColumn
        //
        commentsColumn.Text = "COMMENTS";
        commentsColumn.Width = 200;
        //
        // stateColumn
        //
        stateColumn.Text = "STATE";
        stateColumn.Width = 90;
        //
        // healthColumn
        //
        healthColumn.Text = "SITUACAO";
        healthColumn.Width = 180;
        //
        // jobContextMenu
        //
        jobContextMenu.Name = "jobContextMenu";
        jobContextMenu.Size = new Size(61, 4);
        jobContextMenu.Opening += JobContextMenu_Opening;
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
        // SchedulerJobsForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 661);
        Controls.Add(jobsListView);
        Controls.Add(toolbarPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 400);
        Name = "SchedulerJobsForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Jobs de banco";
        Load += SchedulerJobsForm_Load;
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
    private Button cancelButton;
    private Label listCountLabel;
    private Label overdueLegendLabel;
    private Label longRunningLegendLabel;
    private Label disabledLegendLabel;
    private ListView jobsListView;
    private ColumnHeader createdColumn;
    private ColumnHeader lastDdlColumn;
    private ColumnHeader ownerColumn;
    private ColumnHeader nameColumn;
    private ColumnHeader actionColumn;
    private ColumnHeader enabledColumn;
    private ColumnHeader jobTypeColumn;
    private ColumnHeader programOwnerColumn;
    private ColumnHeader programNameColumn;
    private ColumnHeader scheduleOwnerColumn;
    private ColumnHeader scheduleNameColumn;
    private ColumnHeader scheduleTypeColumn;
    private ColumnHeader repeatIntervalColumn;
    private ColumnHeader lastStartColumn;
    private ColumnHeader lastDurationColumn;
    private ColumnHeader nextRunColumn;
    private ColumnHeader runCountColumn;
    private ColumnHeader failureCountColumn;
    private ColumnHeader commentsColumn;
    private ColumnHeader stateColumn;
    private ColumnHeader healthColumn;
    private ContextMenuStrip jobContextMenu;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
