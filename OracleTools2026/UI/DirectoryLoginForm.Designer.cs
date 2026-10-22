namespace OracleTools2026.UI;

partial class DirectoryLoginForm
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
        domainLabel = new Label();
        domainTextBox = new TextBox();
        savedConnectionsButton = new Button();
        portLabel = new Label();
        portTextBox = new TextBox();
        userLabel = new Label();
        userTextBox = new TextBox();
        passwordLabel = new Label();
        passwordTextBox = new TextBox();
        windowsCredentialsLabel = new Label();
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
        mainLayout.Controls.Add(domainLabel, 0, 0);
        mainLayout.Controls.Add(domainTextBox, 1, 0);
        mainLayout.Controls.Add(savedConnectionsButton, 2, 0);
        mainLayout.Controls.Add(portLabel, 0, 1);
        mainLayout.Controls.Add(portTextBox, 1, 1);
        mainLayout.Controls.Add(userLabel, 0, 2);
        mainLayout.Controls.Add(userTextBox, 1, 2);
        mainLayout.Controls.Add(passwordLabel, 0, 3);
        mainLayout.Controls.Add(passwordTextBox, 1, 3);
        mainLayout.Controls.Add(windowsCredentialsLabel, 1, 4);
        mainLayout.Controls.Add(saveConnectionCheckBox, 1, 5);
        mainLayout.Controls.Add(savePasswordCheckBox, 1, 6);
        mainLayout.Controls.Add(buttonsPanel, 0, 7);
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Location = new Point(12, 12);
        mainLayout.Name = "mainLayout";
        mainLayout.RowCount = 8;
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.SetColumnSpan(windowsCredentialsLabel, 2);
        mainLayout.SetColumnSpan(buttonsPanel, 3);
        mainLayout.Size = new Size(440, 260);
        mainLayout.TabIndex = 0;
        //
        // domainLabel
        //
        domainLabel.Anchor = AnchorStyles.Left;
        domainLabel.AutoSize = true;
        domainLabel.Name = "domainLabel";
        domainLabel.TabIndex = 0;
        domainLabel.Text = "&Domínio:";
        //
        // domainTextBox
        //
        domainTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        domainTextBox.Name = "domainTextBox";
        domainTextBox.PlaceholderText = "em branco usa o domínio do computador";
        domainTextBox.TabIndex = 1;
        //
        // savedConnectionsButton
        //
        savedConnectionsButton.AutoSize = true;
        savedConnectionsButton.Name = "savedConnectionsButton";
        savedConnectionsButton.TabIndex = 10;
        savedConnectionsButton.Text = "Conexões salvas ▾";
        savedConnectionsButton.UseVisualStyleBackColor = true;
        savedConnectionsButton.Click += SavedConnectionsButton_Click;
        //
        // portLabel
        //
        portLabel.Anchor = AnchorStyles.Left;
        portLabel.AutoSize = true;
        portLabel.Name = "portLabel";
        portLabel.TabIndex = 0;
        portLabel.Text = "P&orta:";
        //
        // portTextBox
        //
        portTextBox.Anchor = AnchorStyles.Left;
        portTextBox.Name = "portTextBox";
        portTextBox.Size = new Size(80, 23);
        portTextBox.TabIndex = 2;
        //
        // userLabel
        //
        userLabel.Anchor = AnchorStyles.Left;
        userLabel.AutoSize = true;
        userLabel.Name = "userLabel";
        userLabel.TabIndex = 0;
        userLabel.Text = "&Usuário:";
        //
        // userTextBox
        //
        userTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        userTextBox.Name = "userTextBox";
        userTextBox.PlaceholderText = "DOMINIO\\usuario ou usuario@dominio";
        userTextBox.TabIndex = 3;
        //
        // passwordLabel
        //
        passwordLabel.Anchor = AnchorStyles.Left;
        passwordLabel.AutoSize = true;
        passwordLabel.Name = "passwordLabel";
        passwordLabel.TabIndex = 0;
        passwordLabel.Text = "&Senha:";
        //
        // passwordTextBox
        //
        passwordTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        passwordTextBox.Name = "passwordTextBox";
        passwordTextBox.TabIndex = 4;
        passwordTextBox.UseSystemPasswordChar = true;
        //
        // windowsCredentialsLabel
        //
        windowsCredentialsLabel.AutoSize = true;
        windowsCredentialsLabel.ForeColor = SystemColors.GrayText;
        windowsCredentialsLabel.Margin = new Padding(3, 0, 3, 6);
        windowsCredentialsLabel.Name = "windowsCredentialsLabel";
        windowsCredentialsLabel.TabIndex = 0;
        windowsCredentialsLabel.Text = "Deixe usuário e senha em branco para usar a conta do Windows.";
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
        buttonsPanel.TabIndex = 7;
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 28);
        cancelButton.TabIndex = 9;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // okButton
        //
        okButton.Name = "okButton";
        okButton.Size = new Size(90, 28);
        okButton.TabIndex = 8;
        okButton.Text = "Conectar";
        okButton.UseVisualStyleBackColor = true;
        okButton.Click += OkButton_Click;
        //
        // savedConnectionsMenu
        //
        savedConnectionsMenu.Name = "savedConnectionsMenu";
        savedConnectionsMenu.Size = new Size(61, 4);
        //
        // DirectoryLoginForm
        //
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        CancelButton = cancelButton;
        ClientSize = new Size(464, 284);
        Controls.Add(mainLayout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(480, 0);
        Name = "DirectoryLoginForm";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Conectar no Active Directory";
        Load += DirectoryLoginForm_Load;
        mainLayout.ResumeLayout(false);
        mainLayout.PerformLayout();
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel mainLayout;
    private Label domainLabel;
    private TextBox domainTextBox;
    private Button savedConnectionsButton;
    private Label portLabel;
    private TextBox portTextBox;
    private Label userLabel;
    private TextBox userTextBox;
    private Label passwordLabel;
    private TextBox passwordTextBox;
    private Label windowsCredentialsLabel;
    private CheckBox saveConnectionCheckBox;
    private CheckBox savePasswordCheckBox;
    private FlowLayoutPanel buttonsPanel;
    private Button cancelButton;
    private Button okButton;
    private ContextMenuStrip savedConnectionsMenu;
}
