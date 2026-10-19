using System.Collections;
using System.Globalization;

namespace OracleTools2026.UI;

public sealed class ListViewColumnSorter : IComparer
{
    public const string DateFormat = "dd/MM/yy HH:mm:ss";

    public int SortColumn { get; set; }

    public SortOrder Order { get; set; } = SortOrder.None;

    public void Toggle(int column)
    {
        if (column == SortColumn)
        {
            Order = Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        }
        else
        {
            SortColumn = column;
            Order = SortOrder.Ascending;
        }
    }

    public void Reset()
    {
        SortColumn = 0;
        Order = SortOrder.None;
    }

    public int Compare(object? x, object? y)
    {
        if (Order == SortOrder.None || x is not ListViewItem itemX || y is not ListViewItem itemY)
        {
            return 0;
        }

        var textX = GetText(itemX);
        var textY = GetText(itemY);

        var result = TryParseDate(textX, out var dateX) && TryParseDate(textY, out var dateY)
            ? dateX.CompareTo(dateY)
            : string.Compare(textX, textY, StringComparison.CurrentCultureIgnoreCase);

        return Order == SortOrder.Descending ? -result : result;
    }

    private string GetText(ListViewItem item) =>
        SortColumn < item.SubItems.Count ? item.SubItems[SortColumn].Text : "";

    private static bool TryParseDate(string text, out DateTime value) =>
        DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
}
