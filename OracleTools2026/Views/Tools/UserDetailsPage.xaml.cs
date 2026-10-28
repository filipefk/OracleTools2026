using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.UserDetails;
using Windows.System;

namespace OracleTools2026.Views.Tools;

public sealed partial class UserDetailsPage : ToolPage
{
    public UserDetailsPage()
    {
        ViewModel = App.GetService<UserDetailsViewModel>();
        InitializeComponent();
        ViewModel.UserSearched += (_, _) => UserBox.SelectAll();
    }

    public UserDetailsViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    // Marcadores como "(Nenhuma role)" aparecem esmaecidos.
    public static double NodeOpacity(bool isMuted) => isMuted ? 0.6 : 1;

    private void SectionSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var permissions = sender.SelectedItem == PermissionsSection;
        PermissionsPanel.Visibility = permissions ? Visibility.Visible : Visibility.Collapsed;
        LocatePanel.Visibility = permissions ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UserBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            ViewModel.SearchUserCommand.Execute(null);
        }
    }

    private void LocateBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            ViewModel.LocateUserCommand.Execute(null);
        }
    }

    // Nós sob demanda carregam os filhos ao serem expandidos.
    private async void UserTree_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        if (args.Item is UserTreeNode { IsLazy: true } node)
        {
            await ViewModel.ExpandNodeAsync(node);
        }
    }

    private void UserTree_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args) =>
        ViewModel.SelectedNode = sender.SelectedItem as UserTreeNode;

    private void CopyAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        ViewModel.CopyNodeTextCommand.Execute(null);
    }

    // O menu é montado para o nó clicado com o botão direito (que passa a ser o selecionado).
    private void NodeMenu_Opening(object? sender, object e)
    {
        if (sender is not MenuFlyout menu)
        {
            return;
        }

        menu.Items.Clear();
        var node = (menu.Target as FrameworkElement)?.DataContext as UserTreeNode;
        if (ViewModel.IsBusy)
        {
            menu.Add("Aguarde o fim da operação em andamento", null, () => { }).IsEnabled = false;
            return;
        }

        if (node is not null)
        {
            UserTree.SelectedItem = node;
            ViewModel.SelectedNode = node;

            if (UserDetailsViewModel.CanOpenDdl(node))
            {
                menu.Add($"Abrir DDL de {node.Grant!.QualifiedName} no editor", Glyphs.Code, () => ViewModel.OpenObjectDdlCommand.Execute(node));
                menu.AddSeparator();
            }

            menu.Add("Copiar texto", Glyphs.Copy, () => ViewModel.CopyNodeTextCommand.Execute(node));
        }

        if (ViewModel.RootNode is not null)
        {
            menu.AddSeparator();
            menu.Add("Expandir tudo", Glyphs.Expand, () => ViewModel.ExpandAllCommand.Execute(null));
            menu.Add("Gerar relatório", Glyphs.Report, () => ViewModel.GenerateReportCommand.Execute(null));
            menu.Add("Extrair script do usuário", Glyphs.OpenFile, () => ViewModel.ExtractUserScriptCommand.Execute(null));
        }
    }
}
