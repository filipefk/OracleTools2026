using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OracleTools2026.Models;
using OracleTools2026.ViewModels.Dialogs;

namespace OracleTools2026.Views.Dialogs;

public sealed partial class DirectoryLoginDialog : ContentDialog
{
    private SavedDirectoryConnection? _pendingRemoval;

    public DirectoryLoginDialog(DirectoryConnection? current)
    {
        ViewModel = App.GetService<DirectoryLoginViewModel>();
        ViewModel.Initialize(current);
        InitializeComponent();
    }

    public DirectoryLoginViewModel ViewModel { get; }

    private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        Control target = ViewModel.Domain.Length == 0 ? DomainBox : ViewModel.User.Length == 0 ? UserBox : PasswordInput;
        target.Focus(FocusState.Programmatic);
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            args.Cancel = !await ViewModel.ConnectAsync();
        }
        finally
        {
            deferral.Complete();
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

    private void ApplySavedConnection(SavedDirectoryConnection connection)
    {
        ViewModel.ApplySavedConnection(connection);
        if (!connection.HasPassword && !connection.Connection.UsesWindowsCredentials)
        {
            PasswordInput.Focus(FocusState.Programmatic);
        }
    }

    private void AskRemoveSavedConnection(SavedDirectoryConnection connection)
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
