namespace OracleTools2026.UI;

partial class DatabaseInfoForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseInfoForm));
        detailsListView = new ListView();
        nameColumn = new ColumnHeader();
        valueColumn = new ColumnHeader();
        buttonsPanel = new FlowLayoutPanel();
        closeButton = new Button();
        copyButton = new Button();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        // 
        // detailsListView
        // 
        detailsListView.Columns.AddRange(new ColumnHeader[] { nameColumn, valueColumn });
        detailsListView.Dock = DockStyle.Fill;
        detailsListView.FullRowSelect = true;
        detailsListView.GridLines = true;
        detailsListView.Location = new Point(12, 12);
        detailsListView.Name = "detailsListView";
        detailsListView.Size = new Size(636, 494);
        detailsListView.TabIndex = 0;
        detailsListView.UseCompatibleStateImageBehavior = false;
        detailsListView.View = View.Details;
        // 
        // nameColumn
        // 
        nameColumn.Text = "Propriedade";
        nameColumn.Width = 200;
        // 
        // valueColumn
        // 
        valueColumn.Text = "Valor";
        valueColumn.Width = 400;
        // 
        // buttonsPanel
        // 
        buttonsPanel.AutoSize = true;
        buttonsPanel.Controls.Add(closeButton);
        buttonsPanel.Controls.Add(copyButton);
        buttonsPanel.Dock = DockStyle.Bottom;
        buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonsPanel.Location = new Point(12, 506);
        buttonsPanel.Name = "buttonsPanel";
        buttonsPanel.Padding = new Padding(0, 8, 0, 0);
        buttonsPanel.Size = new Size(636, 42);
        buttonsPanel.TabIndex = 1;
        // 
        // closeButton
        // 
        closeButton.DialogResult = DialogResult.Cancel;
        closeButton.Location = new Point(543, 11);
        closeButton.Name = "closeButton";
        closeButton.Size = new Size(90, 28);
        closeButton.TabIndex = 2;
        closeButton.Text = "&Fechar";
        closeButton.UseVisualStyleBackColor = true;
        // 
        // copyButton
        // 
        copyButton.Location = new Point(447, 11);
        copyButton.Name = "copyButton";
        copyButton.Size = new Size(90, 28);
        copyButton.TabIndex = 1;
        copyButton.Text = "&Copiar";
        copyButton.UseVisualStyleBackColor = true;
        copyButton.Click += CopyButton_Click;
        // 
        // DatabaseInfoForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = closeButton;
        ClientSize = new Size(660, 560);
        Controls.Add(detailsListView);
        Controls.Add(buttonsPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimizeBox = false;
        Name = "DatabaseInfoForm";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Informações do banco";
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ListView detailsListView;
    private ColumnHeader nameColumn;
    private ColumnHeader valueColumn;
    private FlowLayoutPanel buttonsPanel;
    private Button closeButton;
    private Button copyButton;
}
