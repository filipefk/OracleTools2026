using Microsoft.UI.Xaml;
using OracleTools2026.Models;
using OracleTools2026.Views;

namespace OracleTools2026.UI;

// Janelas abertas no processo: a principal (menu) e as ferramentas abertas em janela própria.
// Todas compartilham a conexão do IOracleSession.
public sealed class WindowService
{
    private readonly List<Window> _windows = [];
    private Window? _lastActive;

    public MainWindow? MainWindow { get; private set; }

    public IReadOnlyList<ToolWindow> ToolWindows => _windows.OfType<ToolWindow>().ToList();

    public Window? ActiveWindow => _lastActive ?? _windows.LastOrDefault();

    public void ShowMainWindow()
    {
        MainWindow ??= Track(new MainWindow());
        MainWindow.Activate();
    }

    public ToolWindow OpenToolWindow(ToolDefinition tool, string? argument = null)
    {
        var window = Track(new ToolWindow(tool, argument));
        window.Activate();
        return window;
    }

    public Window? FindWindow(XamlRoot? xamlRoot) =>
        xamlRoot is null ? null : _windows.FirstOrDefault(w => w.Content?.XamlRoot == xamlRoot);

    public void CloseToolWindows()
    {
        foreach (var window in ToolWindows)
        {
            window.Close();
        }
    }

    public async Task ShowErrorInActiveWindowAsync(string message)
    {
        if (ActiveWindow is { } window)
        {
            await DialogService.ShowMessageAsync(window, "Oracle Tools", message);
        }
    }

    private T Track<T>(T window) where T : Window
    {
        _windows.Add(window);
        window.Activated += (_, e) =>
        {
            if (e.WindowActivationState != WindowActivationState.Deactivated)
            {
                _lastActive = window;
            }
        };
        window.Closed += (_, _) =>
        {
            _windows.Remove(window);
            if (ReferenceEquals(_lastActive, window))
            {
                _lastActive = null;
            }

            if (ReferenceEquals(MainWindow, window))
            {
                MainWindow = null;
            }
        };

        return window;
    }
}
