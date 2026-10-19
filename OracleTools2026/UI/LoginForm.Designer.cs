namespace OracleTools2026.UI;

partial class LoginForm
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
        mainLayout = new TableLayoutPanel();
        userLabel = new Label();
        userTextBox = new TextBox();
        passwordLabel = new Label();
        passwordTextBox = new TextBox();
        dataSourceLabel = new Label();
        dataSourceComboBox = new ComboBox();
        locateTnsNamesButton = new Button();
        savedConnectionsButton = new Button();
        saveConnectionCheckBox = new CheckBox();
        savePasswordCheckBox = new CheckBox();
        buttonsPanel = new FlowLayoutPanel();
        cancelButton = new Button();
        okButton = new Button();
        savedConnectionsMenu = new ContextMenuStrip(components);
        mainLayout.SuspendLayout();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        //
        // mainLayout
        //
        mainLayout.AutoSize = true;
        mainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        mainLayout.ColumnCount = 3;
        mainLayout.ColumnStyles.Add(new ColumnStyle());
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.ColumnStyles.Add(new ColumnStyle());
        mainLayout.Controls.Add(userLabel, 0, 0);
        mainLayout.Controls.Add(userTextBox, 1, 0);
        mainLayout.Controls.Add(savedConnectionsButton, 2, 0);
        mainLayout.Controls.Add(passwordLabel, 0, 1);
        mainLayout.Controls.Add(passwordTextBox, 1, 1);
        mainLayout.Controls.Add(dataSourceLabel, 0, 2);
        mainLayout.Controls.Add(dataSourceComboBox, 1, 2);
        mainLayout.Controls.Add(locateTnsNamesButton, 2, 2);
        mainLayout.Controls.Add(saveConnectionCheckBox, 1, 3);
        mainLayout.Controls.Add(savePasswordCheckBox, 1, 4);
        mainLayout.Controls.Add(buttonsPanel, 0, 5);
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Location = new Point(12, 12);
        mainLayout.Name = "mainLayout";
        mainLayout.RowCount = 6;
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.SetColumnSpan(buttonsPanel, 3);
        mainLayout.Size = new Size(420, 210);
        mainLayout.TabIndex = 0;
        //
        // userLabel
        //
        userLabel.Anchor = AnchorStyles.Left;
        userLabel.AutoSize = true;
        userLabel.Name = "userLabel";
        userLabel.Text = "&Usuário:";
        //
        // userTextBox
        //
        userTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        userTextBox.CharacterCasing = CharacterCasing.Upper;
        userTextBox.Name = "userTextBox";
        userTextBox.TabIndex = 1;
        //
        // savedConnectionsButton
        //
        savedConnectionsButton.AutoSize = true;
        savedConnectionsButton.Name = "savedConnectionsButton";
        savedConnectionsButton.TabIndex = 9;
        savedConnectionsButton.Text = "Conexões salvas ▾";
        savedConnectionsButton.UseVisualStyleBackColor = true;
        savedConnectionsButton.Click += SavedConnectionsButton_Click;
        //
        // passwordLabel
        //
        passwordLabel.Anchor = AnchorStyles.Left;
        passwordLabel.AutoSize = true;
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Text = "&Senha:";
        //
        // passwordTextBox
        //
        passwordTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        passwordTextBox.Name = "passwordTextBox";
        passwordTextBox.TabIndex = 2;
        passwordTextBox.UseSystemPasswordChar = true;
        //
        // dataSourceLabel
        //
        dataSourceLabel.Anchor = AnchorStyles.Left;
        dataSourceLabel.AutoSize = true;
        dataSourceLabel.Name = "dataSourceLabel";
        dataSourceLabel.Text = "&Banco:";
        //
        // dataSourceComboBox
        //
        dataSourceComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        dataSourceComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        dataSourceComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        dataSourceComboBox.Name = "dataSourceComboBox";
        dataSourceComboBox.TabIndex = 3;
        dataSourceComboBox.DropDown += DataSourceComboBox_DropDown;
        //
        // locateTnsNamesButton
        //
        locateTnsNamesButton.AutoSize = true;
        locateTnsNamesButton.Name = "locateTnsNamesButton";
        locateTnsNamesButton.TabIndex = 4;
        locateTnsNamesButton.Text = "tnsnames.ora...";
        locateTnsNamesButton.UseVisualStyleBackColor = true;
        locateTnsNamesButton.Click += LocateTnsNamesButton_Click;
        //
        // saveConnectionCheckBox
        //
        saveConnectionCheckBox.AutoSize = true;
        saveConnectionCheckBox.Name = "saveConnectionCheckBox";
        saveConnectionCheckBox.TabIndex = 5;
        saveConnectionCheckBox.Text = "Salvar conexão";
        saveConnectionCheckBox.UseVisualStyleBackColor = true;
        saveConnectionCheckBox.CheckedChanged += SaveConnectionCheckBox_CheckedChanged;
        //
        // savePasswordCheckBox
        //
        savePasswordCheckBox.AutoSize = true;
        savePasswordCheckBox.Enabled = false;
        savePasswordCheckBox.Name = "savePasswordCheckBox";
        savePasswordCheckBox.TabIndex = 6;
        savePasswordCheckBox.Text = "Salvar senha";
        savePasswordCheckBox.UseVisualStyleBackColor = true;
        //
        // buttonsPanel
        //
        buttonsPanel.AutoSize = true;
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Dock = DockStyle.Fill;
        buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonsPanel.Margin = new Padding(0, 12, 0, 0);
        buttonsPanel.Name = "buttonsPanel";
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 28);
        cancelButton.TabIndex = 8;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // okButton
        //
        okButton.Name = "okButton";
        okButton.Size = new Size(90, 28);
        okButton.TabIndex = 7;
        okButton.Text = "Conectar";
        okButton.UseVisualStyleBackColor = true;
        okButton.Click += OkButton_Click;
        //
        // savedConnectionsMenu
        //
        savedConnectionsMenu.Name = "savedConnectionsMenu";
        //
        // LoginForm
        //
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        CancelButton = cancelButton;
        ClientSize = new Size(444, 234);
        Controls.Add(mainLayout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(460, 0);
        Name = "LoginForm";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Conectar no Oracle";
        Load += LoginForm_Load;
        mainLayout.ResumeLayout(false);
        mainLayout.PerformLayout();
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel mainLayout;
    private Label userLabel;
    private TextBox userTextBox;
    private Label passwordLabel;
    private TextBox passwordTextBox;
    private Label dataSourceLabel;
    private ComboBox dataSourceComboBox;
    private Button locateTnsNamesButton;
    private Button savedConnectionsButton;
    private CheckBox saveConnectionCheckBox;
    private CheckBox savePasswordCheckBox;
    private FlowLayoutPanel buttonsPanel;
    private Button cancelButton;
    private Button okButton;
    private ContextMenuStrip savedConnectionsMenu;
}
