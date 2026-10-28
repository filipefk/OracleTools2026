using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.LockMonitor;
using System.ComponentModel;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class LockMonitorPage : ToolPage
{
    public LockMonitorPage()
    {
        ViewModel = App.GetService<LockMonitorViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(LocksTable, ViewModel.Rows);
        LocksTable.RowContextFlyoutOpening += LocksTable_RowContextFlyoutOpening;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public LockMonitorViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    // As colunas vêm do resultado do script; são recriadas só quando mudam.
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LockMonitorViewModel.Columns))
        {
            return;
        }

        LocksTable.Columns.Clear();
        for (var i = 0; i < ViewModel.Columns.Count; i++)
        {
            LocksTable.Columns.Add(new TableViewTextColumn
            {
                Header = ViewModel.Columns[i],
                Binding = new Binding { Path = new PropertyPath($"Values[{i}]") }
            });
        }
    }

    private void CopyScript1_Click(object sender, RoutedEventArgs e) => ViewModel.CopyScriptCommand.Execute(0);

    private void CopyScript2_Click(object sender, RoutedEventArgs e) => ViewModel.CopyScriptCommand.Execute(1);

    private void LocksTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (e.Flyout is not MenuFlyout menu)
        {
            return;
        }

        menu.Items.Clear();
        if (ViewModel.Rows.HasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(LocksTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(LocksTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
