using OracleTools2026.Models;
using System.Text;

namespace OracleTools2026.UI;

public partial class DatabaseInfoForm : Form
{
    public DatabaseInfoForm()
    {
        InitializeComponent();
    }

    public void ShowDetails(IReadOnlyList<DatabaseProperty> properties)
    {
        detailsListView.BeginUpdate();
        detailsListView.Items.Clear();
        detailsListView.Groups.Clear();

        foreach (var group in properties.GroupBy(p => p.Group))
        {
            var listGroup = detailsListView.Groups.Add(group.Key, group.Key);
            foreach (var property in group)
            {
                detailsListView.Items.Add(new ListViewItem([property.Name, property.Value], listGroup) { Tag = property });
            }
        }

        detailsListView.EndUpdate();
        detailsListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
        detailsListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        var items = detailsListView.SelectedItems.Count > 0
            ? detailsListView.SelectedItems.Cast<ListViewItem>()
            : detailsListView.Items.Cast<ListViewItem>();

        var text = new StringBuilder();
        foreach (var group in items.Select(i => (DatabaseProperty)i.Tag!).GroupBy(p => p.Group))
        {
            if (text.Length > 0)
            {
                text.AppendLine();
            }

            text.AppendLine(group.Key);
            foreach (var property in group)
            {
                text.AppendLine($"{property.Name}: {property.Value}");
            }
        }

        if (text.Length > 0)
        {
            Clipboard.SetText(text.ToString());
        }
    }
}
