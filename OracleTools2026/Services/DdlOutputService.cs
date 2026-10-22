using OracleTools2026.Models;
using System.Diagnostics;
using System.Text;

namespace OracleTools2026.Services;

public sealed class DdlOutputService
{
    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public async Task<int> SaveToFolderAsync(string folder, IEnumerable<DdlExtractionResult> results, CancellationToken cancellationToken = default)
    {
        var saved = 0;
        foreach (var result in results.Where(r => r.Succeeded))
        {
            var targetFolder = Path.Combine(folder, result.Owner, result.ObjectType.Replace(' ', '_'));
            Directory.CreateDirectory(targetFolder);
            await File.WriteAllTextAsync(Path.Combine(targetFolder, result.FileName), result.Source, FileEncoding, cancellationToken);
            saved++;
        }

        return saved;
    }

    // Scripts de usuários vão para a subpasta USUARIOS, como no original.
    public async Task<int> SaveUserScriptsAsync(string folder, IEnumerable<UserScriptResult> results, CancellationToken cancellationToken = default)
    {
        var targetFolder = Path.Combine(folder, "USUARIOS");
        var saved = 0;
        foreach (var result in results.Where(r => r.Succeeded))
        {
            Directory.CreateDirectory(targetFolder);
            await File.WriteAllTextAsync(Path.Combine(targetFolder, ToSafeFileName(result.FileName)), result.Script, FileEncoding, cancellationToken);
            saved++;
        }

        return saved;
    }

    public void CopyToClipboard(string text) => Clipboard.SetText(text);

    public async Task OpenInEditorAsync(string text, string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(Path.GetTempPath(), ToSafeFileName(fileName));
        await File.WriteAllTextAsync(path, text, FileEncoding, cancellationToken);
        OpenWithShell(path);
    }

    public void OpenFolder(string folder) => OpenWithShell(folder);

    public static string Combine(IEnumerable<DdlExtractionResult> results) =>
        string.Join("\r\n\r\n", results.Where(r => r.Succeeded).Select(r => r.Source));

    public static string Combine(IEnumerable<UserScriptResult> results) =>
        string.Join("\r\n\r\n", results.Where(r => r.Succeeded).Select(r => r.Script));

    private static string ToSafeFileName(string fileName) =>
        string.Concat(fileName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private static void OpenWithShell(string path) =>
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
}
