using System.Globalization;
using System.Text;

namespace OracleTools2026.UI;

public static class ListViewExtensions
{
    private const int MaxAutoColumnWidth = 450;

    public static void CopyToClipboard(this ListView listView, IEnumerable<ListViewItem> items)
    {
        var rows = items.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var text = new StringBuilder();
        text.AppendJoin('\t', listView.Columns.Cast<ColumnHeader>().Select(c => c.Text.Trim())).Append("\r\n");
        foreach (var row in rows)
        {
            text.AppendJoin('\t', row.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)).Append("\r\n");
        }

        Clipboard.SetText(text.ToString());
    }

    public static void CopySelectedToClipboard(this ListView listView) =>
        listView.CopyToClipboard(listView.SelectedItems.Cast<ListViewItem>());

    public static void CopyAllToClipboard(this ListView listView) =>
        listView.CopyToClipboard(listView.Items.Cast<ListViewItem>());

    public static void SelectAllItems(this ListView listView)
    {
        listView.BeginUpdate();
        foreach (ListViewItem item in listView.Items)
        {
            item.Selected = true;
        }
        listView.EndUpdate();
    }

    // Ctrl+C copia as linhas selecionadas e Ctrl+A seleciona todas.
    public static bool HandleCopyShortcuts(this ListView listView, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C)
        {
            listView.CopySelectedToClipboard();
        }
        else if (e.Control && e.KeyCode == Keys.A)
        {
            listView.SelectAllItems();
        }
        else
        {
            return false;
        }

        e.SuppressKeyPress = true;
        return true;
    }

    // Ajusta cada coluna ao maior entre o cabeçalho e o conteúdo, limitando colunas de texto muito longo.
    public static void AutoResizeColumnsToContent(this ListView listView)
    {
        listView.BeginUpdate();
        foreach (ColumnHeader column in listView.Columns)
        {
            column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            var headerWidth = column.Width;
            if (listView.Items.Count > 0)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            column.Width = Math.Min(Math.Max(headerWidth, column.Width), listView.LogicalToDeviceUnits(MaxAutoColumnWidth));
        }
        listView.EndUpdate();
    }

    public static string FormatDate(DateTime? value) =>
        value?.ToString(ListViewColumnSorter.DateFormat, CultureInfo.InvariantCulture) ?? "";

    public static string FormatDuration(TimeSpan? value) => value switch
    {
        null => "",
        { Days: > 0 } duration => $"{duration.Days}d {duration:hh\\:mm\\:ss}",
        { } duration => duration.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture)
    };

    // Formato usado nas listas montadas a partir de consultas genéricas (mesmo do original: nulos aparecem como NULL).
    public static string FormatValue(object? value) => value switch
    {
        null => "NULL",
        DateTime date => FormatDate(date),
        DateTimeOffset date => FormatDate(date.DateTime),
        TimeSpan duration => FormatDuration(duration),
        byte[] bytes => Convert.ToHexString(bytes),
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? ""
    };
}
