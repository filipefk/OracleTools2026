namespace OracleTools2026.UI;

partial class ExtractDdlForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractDdlForm));
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        connectMenuItem = new ToolStripMenuItem();
        settingsMenuItem = new ToolStripMenuItem();
        serverInfoMenuItem = new ToolStripMenuItem();
        fileSeparator = new ToolStripSeparator();
        exitMenuItem = new ToolStripMenuItem();
        filterPanel = new FlowLayoutPanel();
        ownerLabel = new Label();
        ownerComboBox = new ComboBox();
        typeLabel = new Label();
        typeComboBox = new ComboBox();
        nameLabel = new Label();
        nameTextBox = new TextBox();
        searchButton = new Button();
        daysLabel = new Label();
        daysNumeric = new NumericUpDown();
        recentButton = new Button();
        invalidButton = new Button();
        recompileButton = new Button();
        mainSplit = new SplitContainer();
        objectsListView = new ListView();
        ownerColumn = new ColumnHeader();
        nameColumn = new ColumnHeader();
        typeColumn = new ColumnHeader();
        createdColumn = new ColumnHeader();
        lastDdlColumn = new ColumnHeader();
        statusColumn = new ColumnHeader();
        errorColumn = new ColumnHeader();
        lastModifiedByColumn = new ColumnHeader();
        objectContextMenu = new ContextMenuStrip(components);
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
        includeHeaderCheckBox = new CheckBox();
        inlineErrorsCheckBox = new CheckBox();
        actionsPanel = new FlowLayoutPanel();
        extractButton = new Button();
        cancelOperationButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        menuStrip.SuspendLayout();
        filterPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)daysNumeric).BeginInit();
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
        menuStrip.Size = new Size(1324, 24);
        menuStrip.TabIndex = 0;
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { connectMenuItem, settingsMenuItem, serverInfoMenuItem, fileSeparator, exitMenuItem });
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
        // settingsMenuItem
        // 
        settingsMenuItem.Name = "settingsMenuItem";
        settingsMenuItem.Size = new Size(193, 22);
        settingsMenuItem.Text = "&Parâmetros...";
        settingsMenuItem.Click += SettingsMenuItem_Click;
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
        // filterPanel
        // 
        filterPanel.AutoSize = true;
        filterPanel.Controls.Add(ownerLabel);
        filterPanel.Controls.Add(ownerComboBox);
        filterPanel.Controls.Add(typeLabel);
        filterPanel.Controls.Add(typeComboBox);
        filterPanel.Controls.Add(nameLabel);
        filterPanel.Controls.Add(nameTextBox);
        filterPanel.Controls.Add(searchButton);
        filterPanel.Controls.Add(daysLabel);
        filterPanel.Controls.Add(daysNumeric);
        filterPanel.Controls.Add(recentButton);
        filterPanel.Controls.Add(invalidButton);
        filterPanel.Controls.Add(recompileButton);
        filterPanel.Dock = DockStyle.Top;
        filterPanel.Location = new Point(0, 24);
        filterPanel.Name = "filterPanel";
        filterPanel.Padding = new Padding(6, 6, 6, 0);
        filterPanel.Size = new Size(1324, 37);
        filterPanel.TabIndex = 1;
        // 
        // ownerLabel
        // 
        ownerLabel.Anchor = AnchorStyles.Left;
        ownerLabel.AutoSize = true;
        ownerLabel.Location = new Point(9, 14);
        ownerLabel.Name = "ownerLabel";
        ownerLabel.Size = new Size(45, 15);
        ownerLabel.TabIndex = 0;
        ownerLabel.Text = "&Owner:";
        // 
        // ownerComboBox
        // 
        ownerComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        ownerComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        ownerComboBox.Location = new Point(60, 9);
        ownerComboBox.Name = "ownerComboBox";
        ownerComboBox.Size = new Size(170, 23);
        ownerComboBox.TabIndex = 1;
        // 
        // typeLabel
        // 
        typeLabel.Anchor = AnchorStyles.Left;
        typeLabel.AutoSize = true;
        typeLabel.Location = new Point(236, 14);
        typeLabel.Name = "typeLabel";
        typeLabel.Size = new Size(34, 15);
        typeLabel.TabIndex = 2;
        typeLabel.Text = "&Tipo:";
        // 
        // typeComboBox
        // 
        typeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        typeComboBox.Location = new Point(276, 9);
        typeComboBox.Name = "typeComboBox";
        typeComboBox.Size = new Size(130, 23);
        typeComboBox.TabIndex = 2;
        // 
        // nameLabel
        // 
        nameLabel.Anchor = AnchorStyles.Left;
        nameLabel.AutoSize = true;
        nameLabel.Location = new Point(412, 14);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(43, 15);
        nameLabel.TabIndex = 3;
        nameLabel.Text = "&Nome:";
        // 
        // nameTextBox
        // 
        nameTextBox.CharacterCasing = CharacterCasing.Upper;
        nameTextBox.Location = new Point(461, 9);
        nameTextBox.Name = "nameTextBox";
        nameTextBox.PlaceholderText = "parte do nome";
        nameTextBox.Size = new Size(180, 23);
        nameTextBox.TabIndex = 3;
        nameTextBox.KeyDown += NameTextBox_KeyDown;
        // 
        // searchButton
        // 
        searchButton.AutoSize = true;
        searchButton.Location = new Point(647, 9);
        searchButton.Margin = new Padding(3, 3, 18, 3);
        searchButton.Name = "searchButton";
        searchButton.Size = new Size(75, 25);
        searchButton.TabIndex = 4;
        searchButton.Text = "&Listar";
        searchButton.UseVisualStyleBackColor = true;
        searchButton.Click += SearchButton_Click;
        // 
        // daysLabel
        // 
        daysLabel.Anchor = AnchorStyles.Left;
        daysLabel.AutoSize = true;
        daysLabel.Location = new Point(743, 14);
        daysLabel.Name = "daysLabel";
        daysLabel.Size = new Size(32, 15);
        daysLabel.TabIndex = 5;
        daysLabel.Text = "&Dias:";
        // 
        // daysNumeric
        // 
        daysNumeric.Location = new Point(781, 9);
        daysNumeric.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
        daysNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        daysNumeric.Name = "daysNumeric";
        daysNumeric.Size = new Size(60, 23);
        daysNumeric.TabIndex = 5;
        daysNumeric.Value = new decimal(new int[] { 15, 0, 0, 0 });
        // 
        // recentButton
        // 
        recentButton.AutoSize = true;
        recentButton.Location = new Point(847, 9);
        recentButton.Name = "recentButton";
        recentButton.Size = new Size(127, 25);
        recentButton.TabIndex = 6;
        recentButton.Text = "Últimas &compilações";
        recentButton.UseVisualStyleBackColor = true;
        recentButton.Click += RecentButton_Click;
        // 
        // invalidButton
        // 
        invalidButton.AutoSize = true;
        invalidButton.Location = new Point(980, 9);
        invalidButton.Name = "invalidButton";
        invalidButton.Size = new Size(95, 25);
        invalidButton.TabIndex = 7;
        invalidButton.Text = "Listar &inválidos";
        invalidButton.UseVisualStyleBackColor = true;
        invalidButton.Click += InvalidButton_Click;
        // 
        // recompileButton
        // 
        recompileButton.AutoSize = true;
        recompileButton.Location = new Point(1081, 9);
        recompileButton.Name = "recompileButton";
        recompileButton.Size = new Size(127, 25);
        recompileButton.TabIndex = 8;
        recompileButton.Text = "&Recompilar inválidos";
        recompileButton.UseVisualStyleBackColor = true;
        recompileButton.Click += RecompileButton_Click;
        // 
        // mainSplit
        // 
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.Location = new Point(0, 61);
        mainSplit.Name = "mainSplit";
        // 
        // mainSplit.Panel1
        // 
        mainSplit.Panel1.Controls.Add(objectsListView);
        mainSplit.Panel1.Controls.Add(listHeaderPanel);
        // 
        // mainSplit.Panel2
        // 
        mainSplit.Panel2.Controls.Add(manualListTextBox);
        mainSplit.Panel2.Controls.Add(manualListLabel);
        mainSplit.Size = new Size(1324, 523);
        mainSplit.SplitterDistance = 1006;
        mainSplit.TabIndex = 2;
        // 
        // objectsListView
        // 
        objectsListView.CheckBoxes = true;
        objectsListView.Columns.AddRange(new ColumnHeader[] { ownerColumn, nameColumn, typeColumn, createdColumn, lastDdlColumn, statusColumn, errorColumn, lastModifiedByColumn });
        objectsListView.ContextMenuStrip = objectContextMenu;
        objectsListView.Dock = DockStyle.Fill;
        objectsListView.FullRowSelect = true;
        objectsListView.GridLines = true;
        objectsListView.Location = new Point(0, 25);
        objectsListView.Name = "objectsListView";
        objectsListView.Size = new Size(1006, 498);
        objectsListView.TabIndex = 1;
        objectsListView.UseCompatibleStateImageBehavior = false;
        objectsListView.View = View.Details;
        objectsListView.ColumnClick += ObjectsListView_ColumnClick;
        objectsListView.ItemChecked += ObjectsListView_ItemChecked;
        objectsListView.DoubleClick += ObjectsListView_DoubleClick;
        objectsListView.KeyDown += ObjectsListView_KeyDown;
        // 
        // ownerColumn
        // 
        ownerColumn.Text = "Owner";
        ownerColumn.Width = 130;
        // 
        // nameColumn
        // 
        nameColumn.Text = "Nome";
        nameColumn.Width = 240;
        // 
        // typeColumn
        // 
        typeColumn.Text = "Tipo";
        typeColumn.Width = 100;
        // 
        // createdColumn
        // 
        createdColumn.Text = "Data criação";
        createdColumn.Width = 120;
        // 
        // lastDdlColumn
        // 
        lastDdlColumn.Text = "Data alteração";
        lastDdlColumn.Width = 120;
        // 
        // statusColumn
        // 
        statusColumn.Text = "Status";
        statusColumn.Width = 70;
        // 
        // errorColumn
        // 
        errorColumn.Text = "Mensagem";
        errorColumn.Width = 200;
        // 
        // lastModifiedByColumn
        // 
        lastModifiedByColumn.Text = "Último alterador";
        lastModifiedByColumn.Width = 140;
        // 
        // objectContextMenu
        // 
        objectContextMenu.Name = "objectContextMenu";
        objectContextMenu.Size = new Size(61, 4);
        objectContextMenu.Opening += ObjectContextMenu_Opening;
        // 
        // listHeaderPanel
        // 
        listHeaderPanel.AutoSize = true;
        listHeaderPanel.Controls.Add(selectAllCheckBox);
        listHeaderPanel.Controls.Add(listCountLabel);
        listHeaderPanel.Dock = DockStyle.Top;
        listHeaderPanel.Location = new Point(0, 0);
        listHeaderPanel.Name = "listHeaderPanel";
        listHeaderPanel.Size = new Size(1006, 25);
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
        listCountLabel.Size = new Size(200, 15);
        listCountLabel.TabIndex = 1;
        listCountLabel.Text = "0 itens listados - 0 itens selecionados";
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
        manualListTextBox.Size = new Size(314, 500);
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
        manualListLabel.Size = new Size(257, 23);
        manualListLabel.TabIndex = 2;
        manualListLabel.Text = "Lista de objetos (OWNER.NOME, um por linha):";
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
        extractionPanel.Location = new Point(0, 584);
        extractionPanel.Name = "extractionPanel";
        extractionPanel.Padding = new Padding(6, 0, 6, 6);
        extractionPanel.RowCount = 1;
        extractionPanel.RowStyles.Add(new RowStyle());
        extractionPanel.Size = new Size(1324, 115);
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
        sourceCheckedRadio.Text = "Objetos marcados";
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
        optionsGroupBox.Size = new Size(871, 103);
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
        optionsLayout.Controls.Add(includeHeaderCheckBox, 1, 1);
        optionsLayout.Controls.Add(inlineErrorsCheckBox, 1, 2);
        optionsLayout.Dock = DockStyle.Fill;
        optionsLayout.Location = new Point(3, 19);
        optionsLayout.Name = "optionsLayout";
        optionsLayout.RowCount = 3;
        optionsLayout.RowStyles.Add(new RowStyle());
        optionsLayout.RowStyles.Add(new RowStyle());
        optionsLayout.RowStyles.Add(new RowStyle());
        optionsLayout.Size = new Size(865, 81);
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
        folderLabel.Text = "&Pasta:";
        // 
        // folderTextBox
        // 
        folderTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        folderTextBox.Location = new Point(47, 4);
        folderTextBox.Name = "folderTextBox";
        folderTextBox.Size = new Size(734, 23);
        folderTextBox.TabIndex = 0;
        // 
        // browseFolderButton
        // 
        browseFolderButton.AutoSize = true;
        browseFolderButton.Location = new Point(787, 3);
        browseFolderButton.Name = "browseFolderButton";
        browseFolderButton.Size = new Size(75, 25);
        browseFolderButton.TabIndex = 1;
        browseFolderButton.Text = "...";
        browseFolderButton.UseVisualStyleBackColor = true;
        browseFolderButton.Click += BrowseFolderButton_Click;
        // 
        // includeHeaderCheckBox
        // 
        includeHeaderCheckBox.AutoSize = true;
        includeHeaderCheckBox.Location = new Point(47, 34);
        includeHeaderCheckBox.Name = "includeHeaderCheckBox";
        includeHeaderCheckBox.Size = new Size(304, 19);
        includeHeaderCheckBox.TabIndex = 2;
        includeHeaderCheckBox.Text = "Incluir cabeçalho com dados do objeto e da conexão";
        includeHeaderCheckBox.UseVisualStyleBackColor = true;
        includeHeaderCheckBox.CheckedChanged += IncludeHeaderCheckBox_CheckedChanged;
        // 
        // inlineErrorsCheckBox
        // 
        inlineErrorsCheckBox.AutoSize = true;
        inlineErrorsCheckBox.Location = new Point(47, 59);
        inlineErrorsCheckBox.Name = "inlineErrorsCheckBox";
        inlineErrorsCheckBox.Size = new Size(298, 19);
        inlineErrorsCheckBox.TabIndex = 3;
        inlineErrorsCheckBox.Text = "Incluir mensagens de erro de compilação nas linhas";
        inlineErrorsCheckBox.UseVisualStyleBackColor = true;
        inlineErrorsCheckBox.CheckedChanged += InlineErrorsCheckBox_CheckedChanged;
        // 
        // actionsPanel
        // 
        actionsPanel.AutoSize = true;
        actionsPanel.Controls.Add(extractButton);
        actionsPanel.Controls.Add(cancelOperationButton);
        actionsPanel.Dock = DockStyle.Fill;
        actionsPanel.FlowDirection = FlowDirection.TopDown;
        actionsPanel.Location = new Point(1179, 3);
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
        extractButton.Text = "&Extrair DDL";
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
        statusStrip.Location = new Point(0, 699);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1324, 22);
        statusStrip.TabIndex = 4;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1087, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Pronto";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(220, 16);
        // 
        // ExtractDdlForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1324, 721);
        Controls.Add(mainSplit);
        Controls.Add(extractionPanel);
        Controls.Add(filterPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(900, 560);
        Name = "ExtractDdlForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Extract DDL Oracle";
        FormClosing += ExtractDdlForm_FormClosing;
        Load += ExtractDdlForm_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        filterPanel.ResumeLayout(false);
        filterPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)daysNumeric).EndInit();
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
    private ToolStripMenuItem settingsMenuItem;
    private ToolStripMenuItem serverInfoMenuItem;
    private ToolStripSeparator fileSeparator;
    private ToolStripMenuItem exitMenuItem;
    private FlowLayoutPanel filterPanel;
    private Label ownerLabel;
    private ComboBox ownerComboBox;
    private Label typeLabel;
    private ComboBox typeComboBox;
    private Label nameLabel;
    private TextBox nameTextBox;
    private Button searchButton;
    private Label daysLabel;
    private NumericUpDown daysNumeric;
    private Button recentButton;
    private Button invalidButton;
    private Button recompileButton;
    private SplitContainer mainSplit;
    private ListView objectsListView;
    private ColumnHeader ownerColumn;
    private ColumnHeader nameColumn;
    private ColumnHeader typeColumn;
    private ColumnHeader createdColumn;
    private ColumnHeader lastDdlColumn;
    private ColumnHeader statusColumn;
    private ColumnHeader errorColumn;
    private ColumnHeader lastModifiedByColumn;
    private ContextMenuStrip objectContextMenu;
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
    private CheckBox includeHeaderCheckBox;
    private CheckBox inlineErrorsCheckBox;
    private FlowLayoutPanel actionsPanel;
    private Button extractButton;
    private Button cancelOperationButton;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
}
