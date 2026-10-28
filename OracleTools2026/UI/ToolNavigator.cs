using OracleTools2026.Presentation;

namespace OracleTools2026.UI;

// Abre uma ferramenta a partir de outra: na janela principal, se houver; senão (ferramenta aberta pela
// linha de comando), numa janela própria.
public sealed class ToolNavigator(WindowService windows) : IToolNavigator
{
    public void Open(string command, string? argument = null)
    {
        var tool = ToolCatalog.Find(command) ?? throw new InvalidOperationException($"Ferramenta {command} não está no catálogo.");
        if (windows.MainWindow is { } mainWindow)
        {
            mainWindow.NavigateTo(tool, argument);
            mainWindow.Activate();
        }
        else
        {
            windows.OpenToolWindow(tool, argument);
        }
    }
}
