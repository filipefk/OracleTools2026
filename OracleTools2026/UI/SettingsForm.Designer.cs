namespace OracleTools2026.UI;

partial class SettingsForm
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
        settingsGrid = new DataGridView();
        nameColumn = new DataGridViewTextBoxColumn();
        descriptionColumn = new DataGridViewTextBoxColumn();
        valueColumn = new DataGridViewTextBoxColumn();
        buttonsPanel = new FlowLayoutPanel();
        cancelButton = new Button();
        saveButton = new Button();
        ((System.ComponentModel.ISupportInitialize)settingsGrid).BeginInit();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        //
        // settingsGrid
        //
        settingsGrid.AllowUserToAddRows = false;
        settingsGrid.AllowUserToDeleteRows = false;
        settingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        settingsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        settingsGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, descriptionColumn, valueColumn });
        settingsGrid.Dock = DockStyle.Fill;
        settingsGrid.Name = "settingsGrid";
        settingsGrid.RowHeadersVisible = false;
        settingsGrid.TabIndex = 0;
        //
        // nameColumn
        //
        nameColumn.HeaderText = "Nome";
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        nameColumn.Visible = false;
        //
        // descriptionColumn
        //
        descriptionColumn.FillWeight = 45F;
        descriptionColumn.HeaderText = "Parâmetro";
        descriptionColumn.Name = "descriptionColumn";
        descriptionColumn.ReadOnly = true;
        //
        // valueColumn
        //
        valueColumn.FillWeight = 55F;
        valueColumn.HeaderText = "Valor";
        valueColumn.Name = "valueColumn";
        //
        // buttonsPanel
        //
        buttonsPanel.AutoSize = true;
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(saveButton);
        buttonsPanel.Dock = DockStyle.Bottom;
        buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonsPanel.Name = "buttonsPanel";
        buttonsPanel.Padding = new Padding(0, 8, 0, 0);
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 28);
        cancelButton.TabIndex = 2;
        cancelButton.Text = "Cancelar";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // saveButton
        //
        saveButton.Name = "saveButton";
        saveButton.Size = new Size(90, 28);
        saveButton.TabIndex = 1;
        saveButton.Text = "Salvar";
        saveButton.UseVisualStyleBackColor = true;
        saveButton.Click += SaveButton_Click;
        //
        // SettingsForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(760, 380);
        Controls.Add(settingsGrid);
        Controls.Add(buttonsPanel);
        MinimizeBox = false;
        Name = "SettingsForm";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Parâmetros do Oracle Tools";
        Load += SettingsForm_Load;
        ((System.ComponentModel.ISupportInitialize)settingsGrid).EndInit();
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private DataGridView settingsGrid;
    private DataGridViewTextBoxColumn nameColumn;
    private DataGridViewTextBoxColumn descriptionColumn;
    private DataGridViewTextBoxColumn valueColumn;
    private FlowLayoutPanel buttonsPanel;
    private Button cancelButton;
    private Button saveButton;
}
