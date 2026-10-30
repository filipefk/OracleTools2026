using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using OracleTools2026.Models;
using OracleTools2026.ViewModels.Dialogs;

namespace OracleTools2026.Views.Dialogs;

public sealed partial class LoginDialog : ContentDialog
{
    private readonly Window _window;
    private SavedConnection? _pendingRemoval;
    private bool _askedForTnsNames;

    public LoginDialog(Window window)
    {
        _window = window;
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        SyncModeBar();
    }

    public LoginViewModel ViewModel { get; }

    // A ordem dos itens do SelectorBar é a mesma do enum ConnectionMode.
    private void SyncModeBar() => ModeBar.SelectedItem = ModeBar.Items[(int)ViewModel.Mode];

    private void ModeBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var index = sender.Items.IndexOf(sender.SelectedItem);
        if (index >= 0)
        {
            ViewModel.Mode = (ConnectionMode)index;
        }
    }

    private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        Control target = ViewModel.User.Length == 0 ? UserBox : PasswordInput;
        target.Focus(FocusState.Programmatic);
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            // O diálogo continua aberto se a conexão falhar; o erro aparece na InfoBar.
            args.Cancel = !await ViewModel.ConnectAsync();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void DataSourceBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ShowSuggestions(sender.Text);
        }
    }

    private void DataSourceBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) =>
        ViewModel.DataSource = args.SelectedItem as string ?? ViewModel.DataSource;

    // Modo TNS sem o tnsnames.ora configurado: pede o arquivo na primeira vez que o campo recebe o foco (como o original).
    private async void DataSourceBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (ViewModel.DataSources.Count == 0 && !ViewModel.HasTnsNames && !_askedForTnsNames)
        {
            _askedForTnsNames = true;
            await SelectTnsNamesAsync();
        }

        ShowSuggestions("");
    }

    private void ShowSuggestions(string text)
    {
        var filter = text.Trim();
        DataSourceBox.ItemsSource = ViewModel.DataSources
            .Where(d => filter.Length == 0 || d.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
        DataSourceBox.IsSuggestionListOpen = ViewModel.DataSources.Count > 0;
    }

    private async void TnsNamesButton_Click(object sender, RoutedEventArgs e) => await SelectTnsNamesAsync();

    private async Task SelectTnsNamesAsync()
    {
        var picker = new FileOpenPicker(_window.AppWindow.Id)
        {
            Title = "Informe o caminho do arquivo tnsnames.ora",
            FileTypeFilter = { ".ora", "*" }
        };

        if (await picker.PickSingleFileAsync() is { } result)
        {
            ViewModel.SetTnsNamesPath(result.Path);
        }
    }

    private void SavedConnectionsMenu_Opening(object? sender, object e)
    {
        SavedConnectionsMenu.Items.Clear();
        if (ViewModel.SavedConnections.Count == 0)
        {
            SavedConnectionsMenu.Items.Add(new MenuFlyoutItem { Text = "Nenhuma conexão salva", IsEnabled = false });
            return;
        }

        foreach (var connection in ViewModel.SavedConnections)
        {
            var item = new MenuFlyoutItem { Text = connection.Alias };
            item.Click += (_, _) => ApplySavedConnection(connection);
            SavedConnectionsMenu.Items.Add(item);
        }

        var removeMenu = new MenuFlyoutSubItem { Text = "Excluir", Icon = new FontIcon { Glyph = "\uE74D" } };
        foreach (var connection in ViewModel.SavedConnections)
        {
            var item = new MenuFlyoutItem { Text = connection.Alias };
            item.Click += (_, _) => AskRemoveSavedConnection(connection);
            removeMenu.Items.Add(item);
        }

        SavedConnectionsMenu.Items.Add(new MenuFlyoutSeparator());
        SavedConnectionsMenu.Items.Add(removeMenu);
    }

    private void ApplySavedConnection(SavedConnection connection)
    {
        ViewModel.ApplySavedConnection(connection);
        SyncModeBar();
        if (!connection.HasPassword)
        {
            PasswordInput.Focus(FocusState.Programmatic);
        }
    }

    private void AskRemoveSavedConnection(SavedConnection connection)
    {
        _pendingRemoval = connection;
        ConfirmRemoveText.Text = $"Tem certeza que deseja apagar a conexão \"{connection.Alias}\"?";
        ConfirmRemoveFlyout.ShowAt(SavedConnectionsButton);
    }

    private void ConfirmRemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingRemoval is { } connection)
        {
            ViewModel.RemoveSavedConnection(connection);
        }

        _pendingRemoval = null;
        ConfirmRemoveFlyout.Hide();
    }

    private void CancelRemoveButton_Click(object sender, RoutedEventArgs e)
    {
        _pendingRemoval = null;
        ConfirmRemoveFlyout.Hide();
    }
}
