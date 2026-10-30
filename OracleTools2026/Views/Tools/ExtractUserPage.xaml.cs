using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.ExtractUser;
using Windows.System;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class ExtractUserPage : ToolPage
{
    public ExtractUserPage()
    {
        ViewModel = App.GetService<ExtractUserViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(UsersTable, ViewModel.Users);
        UsersTable.RowContextFlyoutOpening += UsersTable_RowContextFlyoutOpening;
        UsersTable.CellDoubleTapped += UsersTable_CellDoubleTapped;
    }

    public ExtractUserViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    public Predicate<TableViewConditionalCellStyleContext> IsLocked { get; } =
        context => context.DataItem is DatabaseUserItem { IsLocked: true };

    private void FilterTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            ViewModel.SearchCommand.Execute(null);
        }
    }

    // Depois de trocar a conexão, os profiles são carregados ao abrir a lista.
    private async void ProfileComboBox_DropDownOpened(object sender, object e) => await ViewModel.EnsureProfilesLoadedAsync();

    private void UserDetailsButton_Click(object sender, RoutedEventArgs e) => ViewModel.OpenUserDetailsCommand.Execute(null);

    // Usa o evento da célula: com a tabela somente leitura, a célula marca o duplo clique como
    // tratado e o RowDoubleTapped nunca dispara. A coluna do checkbox fica de fora.
    private void UsersTable_CellDoubleTapped(object? sender, TableViewCellDoubleTappedEventArgs e)
    {
        if (e.Cell.Column is not TableViewTemplateColumn && e.Item is DatabaseUserItem user && ViewModel.IsIdle)
        {
            e.Handled = true;
            ViewModel.ViewScriptCommand.Execute(user);
        }
    }

    private void UsersTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (ViewModel.IsBusy || e.Flyout is not MenuFlyout menu)
        {
            e.Handled = true;
            return;
        }

        menu.Items.Clear();
        if (ViewModel.Users.SelectedItem is { } user)
        {
            menu.Add($"Ver script de criação do usuário {user.Name}", Glyphs.OpenFile, () => ViewModel.ViewScriptCommand.Execute(user));
            menu.Add($"Copiar script de criação do usuário {user.Name}", Glyphs.Copy, () => ViewModel.CopyScriptCommand.Execute(user));
            menu.Add($"Ver permissões do usuário {user.Name}", Glyphs.Permissions, () => ViewModel.OpenUserDetailsCommand.Execute(user));
            menu.AddSeparator();
        }

        if (ViewModel.Users.HasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(UsersTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(UsersTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
