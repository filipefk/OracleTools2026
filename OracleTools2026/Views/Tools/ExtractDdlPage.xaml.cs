using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.ExtractDdl;
using Windows.System;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class ExtractDdlPage : ToolPage
{
    public ExtractDdlPage()
    {
        ViewModel = App.GetService<ExtractDdlViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(ObjectsTable, ViewModel.Objects);
        ObjectsTable.RowContextFlyoutOpening += ObjectsTable_RowContextFlyoutOpening;
        ObjectsTable.CellDoubleTapped += ObjectsTable_CellDoubleTapped;
    }

    public ExtractDdlViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    public Predicate<TableViewConditionalCellStyleContext> IsInvalid { get; } =
        context => context.DataItem is DatabaseObjectItem { IsInvalid: true };

    // As opções podem ter sido alteradas na página de Parâmetros.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.ReloadSettings();
    }

    private void FilterBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            ViewModel.SearchCommand.Execute(null);
        }
    }

    // Enter no Owner lista os objetos; escolher uma sugestão só preenche o campo.
    private void OwnerBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is null)
        {
            sender.IsSuggestionListOpen = false;
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private void OwnerBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ShowOwnerSuggestions(sender.Text);
        }
    }

    private void OwnerBox_GotFocus(object sender, RoutedEventArgs e) => ShowOwnerSuggestions("");

    private void OwnerBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) =>
        ViewModel.Owner = args.SelectedItem as string ?? ViewModel.Owner;

    private void ShowOwnerSuggestions(string text)
    {
        var filter = text.Trim();
        OwnerBox.ItemsSource = ViewModel.Owners
            .Where(o => filter.Length == 0 || o.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
        OwnerBox.IsSuggestionListOpen = ViewModel.Owners.Count > 1;
    }

    // Usa o evento da célula: com a tabela somente leitura, a célula marca o duplo clique como
    // tratado e o RowDoubleTapped nunca dispara. A coluna do checkbox fica de fora.
    private void ObjectsTable_CellDoubleTapped(object? sender, TableViewCellDoubleTappedEventArgs e)
    {
        if (e.Cell.Column is not TableViewTemplateColumn && e.Item is DatabaseObjectItem item && ViewModel.IsIdle)
        {
            e.Handled = true;
            ViewModel.OpenDdlCommand.Execute(item);
        }
    }

    private void ObjectsTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (e.Flyout is not MenuFlyout menu)
        {
            return;
        }

        menu.Items.Clear();
        if (ViewModel.Objects.SelectedItem is { } item && ViewModel.IsIdle)
        {
            menu.Add($"Abrir DDL de {item.QualifiedName} no editor", Glyphs.OpenFile, () => ViewModel.OpenDdlCommand.Execute(item));
            menu.Add($"Copiar DDL de {item.QualifiedName}", Glyphs.Copy, () => ViewModel.CopyDdlCommand.Execute(item));
            if (item.HasError)
            {
                menu.Add("Mostrar mensagem de erro", Glyphs.Error, () => ViewModel.ShowErrorMessageCommand.Execute(item));
            }

            menu.AddSeparator();
        }

        if (ViewModel.Objects.HasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(ObjectsTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(ObjectsTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
