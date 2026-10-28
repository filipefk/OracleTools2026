using OracleTools2026.Models;

namespace OracleTools2026.Presentation;

// Serviços de interface ligados à janela em que a ferramenta está aberta (diálogos, seletor de pasta, foco).
public interface IToolHost
{
    Task<bool> AskAsync(string title, string message);
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowLoginAsync();
    Task<DirectoryConnection?> ShowDirectoryLoginAsync(DirectoryConnection? current);
    Task ShowDatabaseInfoAsync(IReadOnlyList<DatabaseProperty> details);
    Task<string?> PickFolderAsync(string title, string? initialFolder);
    void BringToFront();
}
