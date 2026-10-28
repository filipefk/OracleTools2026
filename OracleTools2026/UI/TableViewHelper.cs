using OracleTools2026.Formatting;
using OracleTools2026.Presentation;
using OracleTools2026.ViewModels;
using WinUI.TableView;

namespace OracleTools2026.UI;

public static class TableViewHelper
{
    // Mantém ItemList.Selected em sincronia com a seleção da tabela e faz Ctrl+C copiar as linhas
    // selecionadas com o cabeçalho (como no original).
    public static void Attach<T>(TableView table, ItemList<T> list) where T : class
    {
        table.SelectionChanged += (_, _) => list.Selected = table.SelectedItems.OfType<T>().ToList();
        table.CopyToClipboard += (_, e) =>
        {
            e.Handled = true;
            CopyRows(table, table.SelectedItems.Cast<object>());
        };
    }

    public static void CopySelected(TableView table) => CopyRows(table, table.SelectedItems.Cast<object>());

    public static void CopyAll(TableView table) => CopyRows(table, table.Items.Cast<object>());

    // Copia o texto exibido nas colunas de dados (as colunas de checkbox ficam de fora).
    public static void CopyRows(TableView table, IEnumerable<object> items)
    {
        var rows = items.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var columns = table.Columns.OfType<TableViewBoundColumn>()
            .Where(c => c is not TableViewCheckBoxColumn && c.Visibility == Microsoft.UI.Xaml.Visibility.Visible)
            .OrderBy(c => c.Order ?? int.MaxValue)
            .ToList();

        var text = TabularText.Build(
            columns.Select(c => c.Header?.ToString() ?? ""),
            rows.Select(item => columns.Select(c => c.GetCellContent(item)?.ToString() ?? "")));

        App.GetService<IClipboardService>().SetText(text);
    }
}
