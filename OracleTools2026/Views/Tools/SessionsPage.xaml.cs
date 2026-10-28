using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.Sessions;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class SessionsPage : ToolPage
{
    public SessionsPage()
    {
        ViewModel = App.GetService<SessionsViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(SessionsTable, ViewModel.Sessions);
        SessionsTable.RowContextFlyoutOpening += SessionsTable_RowContextFlyoutOpening;
    }

    public SessionsViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    // Usuário do banco e usuário do SO pertencem a pessoas diferentes.
    public Predicate<TableViewConditionalCellStyleContext> IsDifferentPerson { get; } =
        context => context.DataItem is SessionItem { IsDifferentPerson: true };

    private void DeleteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        if (ViewModel.KillCommand.CanExecute(null))
        {
            ViewModel.KillCommand.Execute(null);
        }
    }

    private void SessionsTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (ViewModel.IsBusy || e.Flyout is not MenuFlyout menu)
        {
            e.Handled = true;
            return;
        }

        menu.Items.Clear();
        var targets = ViewModel.KillTargets;
        if (targets.Count > 0)
        {
            var text = targets.Count == 1 ? "Matar a sessão selecionada" : $"Matar as {targets.Count} sessões selecionadas";
            menu.Add(text, Glyphs.Delete, () => ViewModel.KillCommand.Execute(null));
            menu.AddSeparator();
        }

        if (ViewModel.Sessions.HasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(SessionsTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(SessionsTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
