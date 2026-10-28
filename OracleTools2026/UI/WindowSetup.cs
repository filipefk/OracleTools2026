using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using WinRT.Interop;

namespace OracleTools2026.UI;

// Aparência comum das janelas: Mica, ícone do programa, barra de título integrada e tamanho inicial.
public static class WindowSetup
{
    private static readonly string IconPath = Path.Combine(AppContext.BaseDirectory, "OracleTools.ico");

    public static void Apply(Window window, UIElement titleBar, int width, int height, int minWidth, int minHeight)
    {
        window.SystemBackdrop = new MicaBackdrop();
        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(titleBar);

        if (File.Exists(IconPath))
        {
            window.AppWindow.SetIcon(IconPath);
        }

        // Tamanhos em pixels efetivos, convertidos para a escala do monitor.
        var dpi = NativeMethods.GetDpiForWindow(WindowNative.GetWindowHandle(window));
        var scale = dpi > 0 ? dpi / 96.0 : 1;
        if (window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(minWidth * scale);
            presenter.PreferredMinimumHeight = (int)(minHeight * scale);
        }

        window.AppWindow.Resize(new SizeInt32((int)(width * scale), (int)(height * scale)));
        CenterOnScreen(window);
    }

    private static void CenterOnScreen(Window window)
    {
        if (DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary) is not { } displayArea)
        {
            return;
        }

        var work = displayArea.WorkArea;
        var size = window.AppWindow.Size;
        window.AppWindow.Move(new PointInt32(
            work.X + Math.Max(0, (work.Width - size.Width) / 2),
            work.Y + Math.Max(0, (work.Height - size.Height) / 2)));
    }
}
