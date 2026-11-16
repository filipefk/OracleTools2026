using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using OracleTools2026.Formatting;
using OracleTools2026.Presentation;
using OracleTools2026.ViewModels;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using WinUI.TableView;

namespace OracleTools2026.UI;

public static class TableViewHelper
{
    // Mantém ItemList.Selected em sincronia com a seleção da tabela e faz Ctrl+C copiar as linhas
    // selecionadas com o cabeçalho (como no original).
    private static readonly ConditionalWeakTable<TableView, ClickedCell> ClickedCells = new();

    public static void Attach<T>(TableView table, ItemList<T> list) where T : class
    {
        table.SelectionChanged += (_, _) => list.Selected = table.SelectedItems.OfType<T>().ToList();
        table.CopyToClipboard += (_, e) =>
        {
            e.Handled = true;
            CopyRows(table, table.SelectedItems.Cast<object>());
        };
        TrackClickedCell(table);
        KeepCellWidthsInSync(table);
    }

    public static void CopySelected(TableView table) => CopyRows(table, table.SelectedItems.Cast<object>());

    public static void CopyAll(TableView table) => CopyRows(table, table.Items.Cast<object>());

    public static void CopyCell(TableViewColumn column, object item) =>
        App.GetService<IClipboardService>().SetText(column.GetCellContent(item)?.ToString() ?? "");

    // Itens de cópia comuns aos menus de contexto das listas: célula sob o mouse, linhas
    // selecionadas (seleção da tabela, não o checkbox) e toda a lista.
    public static void AddCopyItems(this MenuFlyout menu, TableView table, object? item, bool hasSelection)
    {
        if (item is not null && GetClickedColumn(table, item) is { } column)
        {
            menu.Add("Copiar célula", Glyphs.Copy, () => CopyCell(column, item));
        }

        if (hasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => CopySelected(table));
        }

        menu.Add("Copiar toda a lista", null, () => CopyAll(table));
    }

    // Copia o texto exibido nas colunas de dados (as colunas de checkbox ficam de fora).
    public static void CopyRows(TableView table, IEnumerable<object> items)
    {
        var rows = items.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var columns = table.Columns.OfType<TableViewBoundColumn>()
            .Where(IsDataColumn)
            .OrderBy(c => c.Order ?? int.MaxValue)
            .ToList();

        var text = TabularText.Build(
            columns.Select(c => c.Header?.ToString() ?? ""),
            rows.Select(item => columns.Select(c => c.GetCellContent(item)?.ToString() ?? "")));

        App.GetService<IClipboardService>().SetText(text);
    }

    private static bool IsDataColumn(TableViewColumn column) =>
        column is TableViewBoundColumn and not TableViewCheckBoxColumn && column.Visibility == Visibility.Visible;

    // O evento RowContextFlyoutOpening não informa a célula; ela é guardada no clique com o botão direito.
    private static void TrackClickedCell(TableView table)
    {
        var clicked = ClickedCells.GetOrCreateValue(table);
        table.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((_, e) =>
        {
            if (!e.GetCurrentPoint(table).Properties.IsRightButtonPressed)
            {
                return;
            }

            var cell = FindAncestor<TableViewCell>(e.OriginalSource as DependencyObject);
            clicked.Column = cell?.Column;
            clicked.Item = cell?.Row?.Content;
        }), handledEventsToo: true);
    }

    private static TableViewColumn? GetClickedColumn(TableView table, object item)
    {
        TableViewColumn? column = null;
        if (ClickedCells.TryGetValue(table, out var clicked) && ReferenceEquals(clicked.Item, item))
        {
            column = clicked.Column;
            clicked.Column = null;
            clicked.Item = null;
        }
        // Menu aberto pelo teclado (Shift+F10): usa a célula atual.
        else if (table.CurrentCellSlot is { } slot && slot.Column >= 0 && slot.Column < table.Columns.Count
                 && slot.Row >= 0 && slot.Row < table.Items.Count && ReferenceEquals(table.Items[slot.Row], item))
        {
            column = table.Columns[slot.Column];
        }

        return column is not null && IsDataColumn(column) ? column : null;
    }

    private static T? FindAncestor<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element is not null and not T)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return element as T;
    }

    private sealed class ClickedCell
    {
        public TableViewColumn? Column { get; set; }

        public object? Item { get; set; }
    }

    // No WinUI.TableView 1.5.1, ao redimensionar a coluna pelo cabeçalho, as células das linhas já
    // exibidas podem ficar com a largura antiga. Reaplica a largura da coluna nas células visíveis
    // sempre que a largura de uma coluna muda.
    private static readonly ConditionalWeakTable<TableView, WidthSync> WidthSyncs = new();

    private static void KeepCellWidthsInSync(TableView table)
    {
        WidthSyncs.GetOrCreateValue(table).Table = table;
        WatchColumns(table);

        if (table.Columns is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged += (_, _) => WatchColumns(table);
        }
    }

    // Colunas criadas depois do Attach (ex.: montadas a partir do resultado de um script) precisam
    // ser registradas: a coleção de colunas do TableView nem sempre avisa as inclusões.
    public static void WatchColumns(TableView table)
    {
        if (!WidthSyncs.TryGetValue(table, out var sync))
        {
            return;
        }

        foreach (var column in table.Columns)
        {
            sync.Watch(column);
        }

        sync.Schedule();
    }

    private sealed class WidthSync
    {
        private readonly ConditionalWeakTable<TableViewColumn, object> _watched = new();
        private bool _pending;

        public TableView? Table { get; set; }

        public void Watch(TableViewColumn column)
        {
            if (_watched.TryGetValue(column, out _))
            {
                return;
            }

            _watched.Add(column, new object());
            column.RegisterPropertyChangedCallback(TableViewColumn.ActualWidthProperty, (_, _) => Schedule());
            column.RegisterPropertyChangedCallback(TableViewColumn.WidthProperty, (_, _) => Schedule());
        }

        public void Schedule()
        {
            if (_pending || Table is not { } table)
            {
                return;
            }

            _pending = true;
            table.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                _pending = false;
                SyncCellWidths(table);
            });
        }
    }

    private static void SyncCellWidths(TableView table)
    {
        if (table.ItemsPanelRoot is not { } panel)
        {
            return;
        }

        foreach (var row in panel.Children.OfType<TableViewRow>())
        {
            var changed = false;
            foreach (var cell in row.Cells)
            {
                if (cell.Column is not { } column || double.IsNaN(column.ActualWidth))
                {
                    continue;
                }

                if (cell.Width != column.ActualWidth || Math.Abs(cell.ActualWidth - column.ActualWidth) > 0.5)
                {
                    cell.Width = column.ActualWidth;
                    cell.InvalidateMeasure();
                    changed = true;
                }
            }

            if (changed)
            {
                row.InvalidateMeasure();
                row.InvalidateArrange();
            }
        }
    }
}
