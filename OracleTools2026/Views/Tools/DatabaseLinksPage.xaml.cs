using Microsoft.UI.Xaml.Controls;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.DatabaseLinks;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class DatabaseLinksPage : ToolPage
{
    public DatabaseLinksPage()
    {
        ViewModel = App.GetService<DatabaseLinksViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(LinksTable, ViewModel.Links);
        LinksTable.RowContextFlyoutOpening += LinksTable_RowContextFlyoutOpening;
    }

    public DatabaseLinksViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    public Predicate<TableViewConditionalCellStyleContext> IsPending { get; } = HasState(DatabaseLinkTestState.Pending);

    public Predicate<TableViewConditionalCellStyleContext> IsOk { get; } = HasState(DatabaseLinkTestState.Ok);

    public Predicate<TableViewConditionalCellStyleContext> IsError { get; } = HasState(DatabaseLinkTestState.Error);

    private static Predicate<TableViewConditionalCellStyleContext> HasState(DatabaseLinkTestState state) =>
        context => context.DataItem is DatabaseLinkItem item && item.TestState == state;

    private void LinksTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (ViewModel.IsBusy || e.Flyout is not MenuFlyout menu)
        {
            e.Handled = true;
            return;
        }

        menu.Items.Clear();
        if (ViewModel.Links.HasSelection)
        {
            menu.Add("Testar DBLINKs selecionados", Glyphs.Test, () => ViewModel.TestSelectedCommand.Execute(null));
            menu.AddSeparator();
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(LinksTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(LinksTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
