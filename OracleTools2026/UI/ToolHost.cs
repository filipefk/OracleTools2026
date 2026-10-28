using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage.Pickers;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using OracleTools2026.Views.Dialogs;
using WinRT.Interop;

namespace OracleTools2026.UI;

// Serviços de interface de uma ferramenta, ligados à janela em que ela está aberta.
public sealed class ToolHost(Window window) : IToolHost
{
    public Window Window { get; } = window;

    public Task<bool> AskAsync(string title, string message) => DialogService.AskAsync(Window, title, message);

    public Task ShowMessageAsync(string title, string message) => DialogService.ShowMessageAsync(Window, title, message);

    public async Task<bool> ShowLoginAsync()
    {
        var dialog = new LoginDialog(Window);
        return await DialogService.ShowAsync(Window, dialog) == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
    }

    public async Task<DirectoryConnection?> ShowDirectoryLoginAsync(DirectoryConnection? current)
    {
        var dialog = new DirectoryLoginDialog(current);
        return await DialogService.ShowAsync(Window, dialog) == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
            ? dialog.ViewModel.Connection
            : null;
    }

    public async Task ShowDatabaseInfoAsync(IReadOnlyList<DatabaseProperty> details) =>
        await DialogService.ShowAsync(Window, new DatabaseInfoDialog(details));

    public async Task<string?> PickFolderAsync(string title, string? initialFolder)
    {
        var picker = new FolderPicker(Window.AppWindow.Id) { Title = title };
        if (!string.IsNullOrWhiteSpace(initialFolder) && Directory.Exists(initialFolder))
        {
            picker.SuggestedFolder = initialFolder;
        }

        var result = await picker.PickSingleFolderAsync();
        return result?.Path;
    }

    // Restaura a janela se estiver minimizada e a traz para a frente das outras.
    public void BringToFront()
    {
        if (Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            if (presenter.State == OverlappedPresenterState.Minimized)
            {
                presenter.Restore();
            }

            presenter.IsAlwaysOnTop = true;
            presenter.IsAlwaysOnTop = false;
        }

        Window.Activate();
        NativeMethods.SetForegroundWindow(WindowNative.GetWindowHandle(Window));
    }
}
