namespace OracleTools2026.UI;

public static class InputDialog
{
    public static string? Show(IWin32Window? owner, string title, string prompt, string initialValue = "")
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12)
        };

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 3,
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        var label = new Label { Text = prompt, AutoSize = true, Margin = new Padding(3, 3, 3, 6) };
        var textBox = new TextBox { Text = initialValue, Width = 320, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, Anchor = AnchorStyles.Right };
        var cancelButton = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };

        layout.Controls.Add(label, 0, 0);
        layout.SetColumnSpan(label, 2);
        layout.Controls.Add(textBox, 0, 1);
        layout.SetColumnSpan(textBox, 2);
        layout.Controls.Add(okButton, 0, 2);
        layout.Controls.Add(cancelButton, 1, 2);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        form.Controls.Add(layout);
        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;

        return form.ShowDialog(owner) == DialogResult.OK ? textBox.Text.Trim() : null;
    }
}
