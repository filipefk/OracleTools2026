using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.ExtractSenior;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class ExtractSeniorPage : ToolPage
{
    public ExtractSeniorPage()
    {
        ViewModel = App.GetService<ExtractSeniorViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(ItemsTable, ViewModel.Items);
    }

    public ExtractSeniorViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    public Predicate<TableViewConditionalCellStyleContext> IsError { get; } =
        context => context.DataItem is SeniorItem { Status: SeniorItemStatus.Error or SeniorItemStatus.NotFound };

    public Predicate<TableViewConditionalCellStyleContext> IsWritten { get; } =
        context => context.DataItem is SeniorItem { Status: SeniorItemStatus.Written };

    public Predicate<TableViewConditionalCellStyleContext> IsMuted { get; } =
        context => context.DataItem is SeniorItem { Status: SeniorItemStatus.NotChecked or SeniorItemStatus.Unchanged };

    // Depois de trocar a conexão, os schemas são carregados ao abrir a lista.
    private async void OwnerComboBox_DropDownOpened(object sender, object e) => await ViewModel.EnsureOwnersLoadedAsync();
}
