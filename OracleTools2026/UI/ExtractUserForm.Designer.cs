namespace OracleTools2026.UI;

partial class ExtractUserForm
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
        userDetailsMenuItem = new ToolStripMenuItem();
        serverInfoMenuItem = new ToolStripMenuItem();
        fileSeparator = new ToolStripSeparator();
        exitMenuItem = new ToolStripMenuItem();
        filterPanel = new FlowLayoutPanel();
        ownerFilterLabel = new Label();
        ownerFilterComboBox = new ComboBox();
        profileLabel = new Label();
        profileComboBox = new ComboBox();
        nameLabel = new Label();
        nameTextBox = new TextBox();
        customFilterLabel = new Label();
        customFilterTextBox = new TextBox();
        searchButton = new Button();
        mainSplit = new SplitContainer();
        usersListView = new ListView();
        nameColumn = new ColumnHeader();
        createdColumn = new ColumnHeader();
        profileColumn = new ColumnHeader();
        statusColumn = new ColumnHeader();
        lockDateColumn = new ColumnHeader();
        expiryDateColumn = new ColumnHeader();
        lastLoginColumn = new ColumnHeader();
        userContextMenu = new ContextMenuStrip(components);
        listHeaderPanel = new FlowLayoutPanel();
        selectAllCheckBox = new CheckBox();
        listCountLabel = new Label();
        manualListTextBox = new TextBox();
        manualListLabel = new Label();
        extractionPanel = new TableLayoutPanel();
        sourceGroupBox = new GroupBox();
        sourcePanel = new FlowLayoutPanel();
        sourceCheckedRadio = new RadioButton();
        sourceAllRadio = new RadioButton();
        sourceManualRadio = new RadioButton();
        destinationGroupBox = new GroupBox();
        destinationPanel = new FlowLayoutPanel();
        destinationFileRadio = new RadioButton();
        destinationClipboardRadio = new RadioButton();
        destinationEditorRadio = new RadioButton();
        optionsGroupBox = new GroupBox();
        optionsLayout = new TableLayoutPanel();
        folderLabel = new Label();
        folderTextBox = new TextBox();
        browseFolderButton = new Button();
        folderHintLabel = new Label();
        actionsPanel = new FlowLayoutPanel();
        extractButton = new Button();
        cancelOperationButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        menuStrip.SuspendLayout();
        filterPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
        mainSplit.Panel1.SuspendLayout();
        mainSplit.Panel2.SuspendLayout();
        mainSplit.SuspendLayout();
        listHeaderPanel.SuspendLayout();
        extractionPanel.SuspendLayout();
        sourceGroupBox.SuspendLayout();
        sourcePanel.SuspendLayout();
        destinationGroupBox.SuspendLayout();
        destinationPanel.SuspendLayout();
        optionsGroupBox.SuspendLayout();
        optionsLayout.SuspendLayout();
        actionsPanel.SuspendLayout();
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
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { connectMenuItem, userDetailsMenuItem, serverInfoMenuItem, fileSeparator, exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(61, 20);
        fileMenu.Text = "&Arquivo";
        //
        // connectMenuItem
        //
        connectMenuItem.Name = "connectMenuItem";
        connectMenuItem.ShortcutKeys = Keys.Control | Keys.L;
        connectMenuItem.Size = new Size(220, 22);
        connectMenuItem.Text = "&Conectar...";
        connectMenuItem.Click += ConnectMenuItem_Click;
        //
        // userDetailsMenuItem
        //
        userDetailsMenuItem.Name = "userDetailsMenuItem";
        userDetailsMenuItem.ShortcutKeys = Keys.Control | Keys.D;
        userDetailsMenuItem.Size = new Size(220, 22);
        userDetailsMenuItem.Text = "&Detalhes do usuário...";
        userDetailsMenuItem.Click += UserDetailsMenuItem_Click;
        //
        // serverInfoMenuItem
        //
        serverInfoMenuItem.Name = "serverInfoMenuItem";
        serverInfoMenuItem.Size = new Size(220, 22);
        serverInfoMenuItem.Text = "&Informações do banco";
        serverInfoMenuItem.Click += ServerInfoMenuItem_Click;
        //
        // fileSeparator
        //
        fileSeparator.Name = "fileSeparator";
        fileSeparator.Size = new Size(217, 6);
        //
        // exitMenuItem
        //
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(220, 22);
        exitMenuItem.Text = "Sai&r";
        exitMenuItem.Click += ExitMenuItem_Click;
        //
        // filterPanel
        //
        filterPanel.AutoSize = true;
        filterPanel.Controls.Add(ownerFilterLabel);
        filterPanel.Controls.Add(ownerFilterComboBox);
        filterPanel.Controls.Add(profileLabel);
        filterPanel.Controls.Add(profileComboBox);
        filterPanel.Controls.Add(nameLabel);
        filterPanel.Controls.Add(nameTextBox);
        filterPanel.Controls.Add(customFilterLabel);
        filterPanel.Controls.Add(customFilterTextBox);
        filterPanel.Controls.Add(searchButton);
        filterPanel.Dock = DockStyle.Top;
        filterPanel.Location = new Point(0, 24);
        filterPanel.Name = "filterPanel";
        filterPanel.Padding = new Padding(6, 6, 6, 0);
        filterPanel.Size = new Size(1184, 37);
        filterPanel.TabIndex = 1;
        //
        // ownerFilterLabel
        //
        ownerFilterLabel.Anchor = AnchorStyles.Left;
        ownerFilterLabel.AutoSize = true;
        ownerFilterLabel.Location = new Point(9, 14);
        ownerFilterLabel.Name = "ownerFilterLabel";
        ownerFilterLabel.Size = new Size(45, 15);
        ownerFilterLabel.TabIndex = 0;
        ownerFilterLabel.Text = "&Owner:";
        //
        // ownerFilterComboBox
        //
        ownerFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        ownerFilterComboBox.Location = new Point(60, 9);
        ownerFilterComboBox.Name = "ownerFilterComboBox";
        ownerFilterComboBox.Size = new Size(140, 23);
        ownerFilterComboBox.TabIndex = 1;
        //
        // profileLabel
        //
        profileLabel.Anchor = AnchorStyles.Left;
        profileLabel.AutoSize = true;
        profileLabel.Location = new Point(206, 14);
        profileLabel.Name = "profileLabel";
        profileLabel.Size = new Size(44, 15);
        profileLabel.TabIndex = 2;
        profileLabel.Text = "&Profile:";
        //
        // profileComboBox
        //
        profileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        profileComboBox.Location = new Point(256, 9);
        profileComboBox.Name = "profileComboBox";
        profileComboBox.Size = new Size(160, 23);
        profileComboBox.TabIndex = 3;
        profileComboBox.DropDown += ProfileComboBox_DropDown;
        //
        // nameLabel
        //
        nameLabel.Anchor = AnchorStyles.Left;
        nameLabel.AutoSize = true;
        nameLabel.Location = new Point(422, 14);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(43, 15);
        nameLabel.TabIndex = 4;
        nameLabel.Text = "&Nome:";
        //
        // nameTextBox
        //
        nameTextBox.CharacterCasing = CharacterCasing.Upper;
        nameTextBox.Location = new Point(471, 9);
        nameTextBox.Name = "nameTextBox";
        nameTextBox.PlaceholderText = "parte do nome";
        nameTextBox.Size = new Size(160, 23);
        nameTextBox.TabIndex = 5;
        nameTextBox.KeyDown += FilterTextBox_KeyDown;
        //
        // customFilterLabel
        //
        customFilterLabel.Anchor = AnchorStyles.Left;
        customFilterLabel.AutoSize = true;
        customFilterLabel.Location = new Point(637, 14);
        customFilterLabel.Name = "customFilterLabel";
        customFilterLabel.Size = new Size(64, 15);
        customFilterLabel.TabIndex = 6;
        customFilterLabel.Text = "&Filtro SQL:";
        //
        // customFilterTextBox
        //
        customFilterTextBox.Location = new Point(707, 9);
        customFilterTextBox.Name = "customFilterTextBox";
        customFilterTextBox.PlaceholderText = "condição extra, sem WHERE nem o primeiro AND";
        customFilterTextBox.Size = new Size(300, 23);
        customFilterTextBox.TabIndex = 7;
        customFilterTextBox.KeyDown += FilterTextBox_KeyDown;
        //
        // searchButton
        //
        searchButton.AutoSize = true;
        searchButton.Location = new Point(1013, 9);
        searchButton.Name = "searchButton";
        searchButton.Size = new Size(75, 25);
        searchButton.TabIndex = 8;
        searchButton.Text = "&Listar";
        searchButton.UseVisualStyleBackColor = true;
        searchButton.Click += SearchButton_Click;
        //
        // mainSplit
        //
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.Location = new Point(0, 61);
        mainSplit.Name = "mainSplit";
        //
        // mainSplit.Panel1
        //
        mainSplit.Panel1.Controls.Add(usersListView);
        mainSplit.Panel1.Controls.Add(listHeaderPanel);
        //
        // mainSplit.Panel2
        //
        mainSplit.Panel2.Controls.Add(manualListTextBox);
        mainSplit.Panel2.Controls.Add(manualListLabel);
        mainSplit.Size = new Size(1184, 463);
        mainSplit.SplitterDistance = 900;
        mainSplit.TabIndex = 2;
        //
        // usersListView
        //
        usersListView.CheckBoxes = true;
        usersListView.Columns.AddRange(new ColumnHeader[] { nameColumn, createdColumn, profileColumn, statusColumn, lockDateColumn, expiryDateColumn, lastLoginColumn });
        usersListView.ContextMenuStrip = userContextMenu;
        usersListView.Dock = DockStyle.Fill;
        usersListView.FullRowSelect = true;
        usersListView.GridLines = true;
        usersListView.Location = new Point(0, 25);
        usersListView.Name = "usersListView";
        usersListView.Size = new Size(900, 438);
        usersListView.TabIndex = 1;
        usersListView.UseCompatibleStateImageBehavior = false;
        usersListView.View = View.Details;
        usersListView.ColumnClick += UsersListView_ColumnClick;
        usersListView.ItemChecked += UsersListView_ItemChecked;
        usersListView.DoubleClick += UsersListView_DoubleClick;
        usersListView.KeyDown += UsersListView_KeyDown;
        //
        // nameColumn
        //
        nameColumn.Text = "Nome";
        nameColumn.Width = 200;
        //
        // createdColumn
        //
        createdColumn.Text = "Criação";
        createdColumn.Width = 120;
        //
        // profileColumn
        //
        profileColumn.Text = "Profile";
        profileColumn.Width = 120;
        //
        // statusColumn
        //
        statusColumn.Text = "Status";
        statusColumn.Width = 120;
        //
        // lockDateColumn
        //
        lockDateColumn.Text = "Data de lock";
        lockDateColumn.Width = 120;
        //
        // expiryDateColumn
        //
        expiryDateColumn.Text = "Data de expiração";
        expiryDateColumn.Width = 120;
        //
        // lastLoginColumn
        //
        lastLoginColumn.Text = "Último login";
        lastLoginColumn.Width = 120;
        //
        // userContextMenu
        //
        userContextMenu.Name = "userContextMenu";
        userContextMenu.Size = new Size(61, 4);
        userContextMenu.Opening += UserContextMenu_Opening;
        //
        // listHeaderPanel
        //
        listHeaderPanel.AutoSize = true;
        listHeaderPanel.Controls.Add(selectAllCheckBox);
        listHeaderPanel.Controls.Add(listCountLabel);
        listHeaderPanel.Dock = DockStyle.Top;
        listHeaderPanel.Location = new Point(0, 0);
        listHeaderPanel.Name = "listHeaderPanel";
        listHeaderPanel.Size = new Size(900, 25);
        listHeaderPanel.TabIndex = 2;
        //
        // selectAllCheckBox
        //
        selectAllCheckBox.AutoSize = true;
        selectAllCheckBox.Location = new Point(3, 3);
        selectAllCheckBox.Name = "selectAllCheckBox";
        selectAllCheckBox.Size = new Size(113, 19);
        selectAllCheckBox.TabIndex = 0;
        selectAllCheckBox.Text = "Selecionar todos";
        selectAllCheckBox.UseVisualStyleBackColor = true;
        selectAllCheckBox.CheckStateChanged += SelectAllCheckBox_CheckStateChanged;
        //
        // listCountLabel
        //
        listCountLabel.Anchor = AnchorStyles.Left;
        listCountLabel.AutoSize = true;
        listCountLabel.Location = new Point(137, 5);
        listCountLabel.Margin = new Padding(18, 0, 3, 0);
        listCountLabel.Name = "listCountLabel";
        listCountLabel.Size = new Size(250, 15);
        listCountLabel.TabIndex = 1;
        listCountLabel.Text = "0 usuários listados - 0 usuários selecionados";
        //
        // manualListTextBox
        //
        manualListTextBox.AcceptsReturn = true;
        manualListTextBox.CharacterCasing = CharacterCasing.Upper;
        manualListTextBox.Dock = DockStyle.Fill;
        manualListTextBox.Font = new Font("Consolas", 9.75F);
        manualListTextBox.Location = new Point(0, 23);
        manualListTextBox.Multiline = true;
        manualListTextBox.Name = "manualListTextBox";
        manualListTextBox.ScrollBars = ScrollBars.Both;
        manualListTextBox.Size = new Size(280, 440);
        manualListTextBox.TabIndex = 1;
        manualListTextBox.WordWrap = false;
        manualListTextBox.KeyDown += ManualListTextBox_KeyDown;
        //
        // manualListLabel
        //
        manualListLabel.AutoSize = true;
        manualListLabel.Dock = DockStyle.Top;
        manualListLabel.Location = new Point(0, 0);
        manualListLabel.Name = "manualListLabel";
        manualListLabel.Padding = new Padding(0, 4, 0, 4);
        manualListLabel.Size = new Size(176, 23);
        manualListLabel.TabIndex = 2;
        manualListLabel.Text = "Lista de usuários (um por linha):";
        //
        // extractionPanel
        //
        extractionPanel.AutoSize = true;
        extractionPanel.ColumnCount = 4;
        extractionPanel.ColumnStyles.Add(new ColumnStyle());
        extractionPanel.ColumnStyles.Add(new ColumnStyle());
        extractionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        extractionPanel.ColumnStyles.Add(new ColumnStyle());
        extractionPanel.Controls.Add(sourceGroupBox, 0, 0);
        extractionPanel.Controls.Add(destinationGroupBox, 1, 0);
        extractionPanel.Controls.Add(optionsGroupBox, 2, 0);
        extractionPanel.Controls.Add(actionsPanel, 3, 0);
        extractionPanel.Dock = DockStyle.Bottom;
        extractionPanel.Location = new Point(0, 524);
        extractionPanel.Name = "extractionPanel";
        extractionPanel.Padding = new Padding(6, 0, 6, 6);
        extractionPanel.RowCount = 1;
        extractionPanel.RowStyles.Add(new RowStyle());
        extractionPanel.Size = new Size(1184, 115);
        extractionPanel.TabIndex = 3;
        //
        // sourceGroupBox
        //
        sourceGroupBox.AutoSize = true;
        sourceGroupBox.Controls.Add(sourcePanel);
        sourceGroupBox.Dock = DockStyle.Fill;
        sourceGroupBox.Location = new Point(9, 3);
        sourceGroupBox.Name = "sourceGroupBox";
        sourceGroupBox.Size = new Size(133, 103);
        sourceGroupBox.TabIndex = 0;
        sourceGroupBox.TabStop = false;
        sourceGroupBox.Text = "Extrair de";
        //
        // sourcePanel
        //
        sourcePanel.AutoSize = true;
        sourcePanel.Controls.Add(sourceCheckedRadio);
        sourcePanel.Controls.Add(sourceAllRadio);
        sourcePanel.Controls.Add(sourceManualRadio);
        sourcePanel.Dock = DockStyle.Fill;
        sourcePanel.FlowDirection = FlowDirection.TopDown;
        sourcePanel.Location = new Point(3, 19);
        sourcePanel.Name = "sourcePanel";
        sourcePanel.Size = new Size(127, 81);
        sourcePanel.TabIndex = 0;
        //
        // sourceCheckedRadio
        //
        sourceCheckedRadio.AutoSize = true;
        sourceCheckedRadio.Checked = true;
        sourceCheckedRadio.Location = new Point(3, 3);
        sourceCheckedRadio.Name = "sourceCheckedRadio";
        sourceCheckedRadio.Size = new Size(121, 19);
        sourceCheckedRadio.TabIndex = 0;
        sourceCheckedRadio.TabStop = true;
        sourceCheckedRadio.Text = "Usuários marcados";
        sourceCheckedRadio.UseVisualStyleBackColor = true;
        //
        // sourceAllRadio
        //
        sourceAllRadio.AutoSize = true;
        sourceAllRadio.Location = new Point(3, 28);
        sourceAllRadio.Name = "sourceAllRadio";
        sourceAllRadio.Size = new Size(84, 19);
        sourceAllRadio.TabIndex = 1;
        sourceAllRadio.Text = "Toda a lista";
        sourceAllRadio.UseVisualStyleBackColor = true;
        //
        // sourceManualRadio
        //
        sourceManualRadio.AutoSize = true;
        sourceManualRadio.Location = new Point(3, 53);
        sourceManualRadio.Name = "sourceManualRadio";
        sourceManualRadio.Size = new Size(95, 19);
        sourceManualRadio.TabIndex = 2;
        sourceManualRadio.Text = "Lista digitada";
        sourceManualRadio.UseVisualStyleBackColor = true;
        //
        // destinationGroupBox
        //
        destinationGroupBox.AutoSize = true;
        destinationGroupBox.Controls.Add(destinationPanel);
        destinationGroupBox.Dock = DockStyle.Fill;
        destinationGroupBox.Location = new Point(148, 3);
        destinationGroupBox.Name = "destinationGroupBox";
        destinationGroupBox.Size = new Size(148, 103);
        destinationGroupBox.TabIndex = 1;
        destinationGroupBox.TabStop = false;
        destinationGroupBox.Text = "Destino";
        //
        // destinationPanel
        //
        destinationPanel.AutoSize = true;
        destinationPanel.Controls.Add(destinationFileRadio);
        destinationPanel.Controls.Add(destinationClipboardRadio);
        destinationPanel.Controls.Add(destinationEditorRadio);
        destinationPanel.Dock = DockStyle.Fill;
        destinationPanel.FlowDirection = FlowDirection.TopDown;
        destinationPanel.Location = new Point(3, 19);
        destinationPanel.Name = "destinationPanel";
        destinationPanel.Size = new Size(142, 81);
        destinationPanel.TabIndex = 0;
        //
        // destinationFileRadio
        //
        destinationFileRadio.AutoSize = true;
        destinationFileRadio.Checked = true;
        destinationFileRadio.Location = new Point(3, 3);
        destinationFileRadio.Name = "destinationFileRadio";
        destinationFileRadio.Size = new Size(119, 19);
        destinationFileRadio.TabIndex = 0;
        destinationFileRadio.TabStop = true;
        destinationFileRadio.Text = "Arquivos na pasta";
        destinationFileRadio.UseVisualStyleBackColor = true;
        destinationFileRadio.CheckedChanged += DestinationRadio_CheckedChanged;
        //
        // destinationClipboardRadio
        //
        destinationClipboardRadio.AutoSize = true;
        destinationClipboardRadio.Location = new Point(3, 28);
        destinationClipboardRadio.Name = "destinationClipboardRadio";
        destinationClipboardRadio.Size = new Size(136, 19);
        destinationClipboardRadio.TabIndex = 1;
        destinationClipboardRadio.Text = "Área de transferência";
        destinationClipboardRadio.UseVisualStyleBackColor = true;
        destinationClipboardRadio.CheckedChanged += DestinationRadio_CheckedChanged;
        //
        // destinationEditorRadio
        //
        destinationEditorRadio.AutoSize = true;
        destinationEditorRadio.Location = new Point(3, 53);
        destinationEditorRadio.Name = "destinationEditorRadio";
        destinationEditorRadio.Size = new Size(101, 19);
        destinationEditorRadio.TabIndex = 2;
        destinationEditorRadio.Text = "Editor de texto";
        destinationEditorRadio.UseVisualStyleBackColor = true;
        destinationEditorRadio.CheckedChanged += DestinationRadio_CheckedChanged;
        //
        // optionsGroupBox
        //
        optionsGroupBox.AutoSize = true;
        optionsGroupBox.Controls.Add(optionsLayout);
        optionsGroupBox.Dock = DockStyle.Fill;
        optionsGroupBox.Location = new Point(302, 3);
        optionsGroupBox.Name = "optionsGroupBox";
        optionsGroupBox.Size = new Size(731, 103);
        optionsGroupBox.TabIndex = 2;
        optionsGroupBox.TabStop = false;
        optionsGroupBox.Text = "Opções";
        //
        // optionsLayout
        //
        optionsLayout.AutoSize = true;
        optionsLayout.ColumnCount = 3;
        optionsLayout.ColumnStyles.Add(new ColumnStyle());
        optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        optionsLayout.ColumnStyles.Add(new ColumnStyle());
        optionsLayout.Controls.Add(folderLabel, 0, 0);
        optionsLayout.Controls.Add(folderTextBox, 1, 0);
        optionsLayout.Controls.Add(browseFolderButton, 2, 0);
        optionsLayout.Controls.Add(folderHintLabel, 1, 1);
        optionsLayout.Dock = DockStyle.Fill;
        optionsLayout.Location = new Point(3, 19);
        optionsLayout.Name = "optionsLayout";
        optionsLayout.RowCount = 2;
        optionsLayout.RowStyles.Add(new RowStyle());
        optionsLayout.RowStyles.Add(new RowStyle());
        optionsLayout.Size = new Size(725, 81);
        optionsLayout.TabIndex = 0;
        //
        // folderLabel
        //
        folderLabel.Anchor = AnchorStyles.Left;
        folderLabel.AutoSize = true;
        folderLabel.Location = new Point(3, 8);
        folderLabel.Name = "folderLabel";
        folderLabel.Size = new Size(38, 15);
        folderLabel.TabIndex = 0;
        folderLabel.Text = "Pas&ta:";
        //
        // folderTextBox
        //
        folderTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        folderTextBox.Location = new Point(47, 4);
        folderTextBox.Name = "folderTextBox";
        folderTextBox.Size = new Size(594, 23);
        folderTextBox.TabIndex = 0;
        //
        // browseFolderButton
        //
        browseFolderButton.AutoSize = true;
        browseFolderButton.Location = new Point(647, 3);
        browseFolderButton.Name = "browseFolderButton";
        browseFolderButton.Size = new Size(75, 25);
        browseFolderButton.TabIndex = 1;
        browseFolderButton.Text = "...";
        browseFolderButton.UseVisualStyleBackColor = true;
        browseFolderButton.Click += BrowseFolderButton_Click;
        //
        // folderHintLabel
        //
        folderHintLabel.AutoSize = true;
        folderHintLabel.ForeColor = SystemColors.GrayText;
        folderHintLabel.Location = new Point(47, 31);
        folderHintLabel.Margin = new Padding(3);
        folderHintLabel.Name = "folderHintLabel";
        folderHintLabel.Size = new Size(330, 15);
        folderHintLabel.TabIndex = 2;
        folderHintLabel.Text = "Os scripts são gravados em USUARIOS\\NOME.sql dentro da pasta.";
        //
        // actionsPanel
        //
        actionsPanel.AutoSize = true;
        actionsPanel.Controls.Add(extractButton);
        actionsPanel.Controls.Add(cancelOperationButton);
        actionsPanel.Dock = DockStyle.Fill;
        actionsPanel.FlowDirection = FlowDirection.TopDown;
        actionsPanel.Location = new Point(1039, 3);
        actionsPanel.Name = "actionsPanel";
        actionsPanel.Padding = new Padding(0, 8, 0, 0);
        actionsPanel.Size = new Size(136, 103);
        actionsPanel.TabIndex = 3;
        //
        // extractButton
        //
        extractButton.Location = new Point(3, 11);
        extractButton.Name = "extractButton";
        extractButton.Size = new Size(130, 32);
        extractButton.TabIndex = 0;
        extractButton.Text = "&Extrair script";
        extractButton.UseVisualStyleBackColor = true;
        extractButton.Click += ExtractButton_Click;
        //
        // cancelOperationButton
        //
        cancelOperationButton.Enabled = false;
        cancelOperationButton.Location = new Point(3, 49);
        cancelOperationButton.Name = "cancelOperationButton";
        cancelOperationButton.Size = new Size(130, 32);
        cancelOperationButton.TabIndex = 1;
        cancelOperationButton.Text = "Cancelar";
        cancelOperationButton.UseVisualStyleBackColor = true;
        cancelOperationButton.Click += CancelOperationButton_Click;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 639);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1184, 22);
        statusStrip.TabIndex = 4;
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
        // ExtractUserForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 661);
        Controls.Add(mainSplit);
        Controls.Add(extractionPanel);
        Controls.Add(filterPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(900, 560);
        Name = "ExtractUserForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Extrair script de usuários";
        Load += ExtractUserForm_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        filterPanel.ResumeLayout(false);
        filterPanel.PerformLayout();
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel1.PerformLayout();
        mainSplit.Panel2.ResumeLayout(false);
        mainSplit.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        listHeaderPanel.ResumeLayout(false);
        listHeaderPanel.PerformLayout();
        extractionPanel.ResumeLayout(false);
        extractionPanel.PerformLayout();
        sourceGroupBox.ResumeLayout(false);
        sourceGroupBox.PerformLayout();
        sourcePanel.ResumeLayout(false);
        sourcePanel.PerformLayout();
        destinationGroupBox.ResumeLayout(false);
        destinationGroupBox.PerformLayout();
        destinationPanel.ResumeLayout(false);
        destinationPanel.PerformLayout();
        optionsGroupBox.ResumeLayout(false);
        optionsGroupBox.PerformLayout();
        optionsLayout.ResumeLayout(false);
        optionsLayout.PerformLayout();
        actionsPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem connectMenuItem;
    private ToolStripMenuItem userDetailsMenuItem;
    private ToolStripMenuItem serverInfoMenuItem;
    private ToolStripSeparator fileSeparator;
    private ToolStripMenuItem exitMenuItem;
    private FlowLayoutPanel filterPanel;
    private Label ownerFilterLabel;
    private ComboBox ownerFilterComboBox;
    private Label profileLabel;
    private ComboBox profileComboBox;
    private Label nameLabel;
    private TextBox nameTextBox;
    private Label customFilterLabel;
    private TextBox customFilterTextBox;
    private Button searchButton;
    private SplitContainer mainSplit;
    private ListView usersListView;
    private ColumnHeader nameColumn;
    private ColumnHeader createdColumn;
    private ColumnHeader profileColumn;
    private ColumnHeader statusColumn;
    private ColumnHeader lockDateColumn;
    private ColumnHeader expiryDateColumn;
    private ColumnHeader lastLoginColumn;
    private ContextMenuStrip userContextMenu;
    private FlowLayoutPanel listHeaderPanel;
    private CheckBox selectAllCheckBox;
    private Label listCountLabel;
    private TextBox manualListTextBox;
    private Label manualListLabel;
    private TableLayoutPanel extractionPanel;
    private GroupBox sourceGroupBox;
    private FlowLayoutPanel sourcePanel;
    private RadioButton sourceCheckedRadio;
    private RadioButton sourceAllRadio;
    private RadioButton sourceManualRadio;
    private GroupBox destinationGroupBox;
    private FlowLayoutPanel destinationPanel;
    private RadioButton destinationFileRadio;
    private RadioButton destinationClipboardRadio;
    private RadioButton destinationEditorRadio;
    private GroupBox optionsGroupBox;
    private TableLayoutPanel optionsLayout;
    private Label folderLabel;
    private TextBox folderTextBox;
    private Button browseFolderButton;
    private Label folderHintLabel;
    private FlowLayoutPanel actionsPanel;
    private Button extractButton;
    private Button cancelOperationButton;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
