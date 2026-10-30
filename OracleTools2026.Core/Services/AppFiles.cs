using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OracleTools2026.Services;

// Arquivos auxiliares (scripts de lock, listas de nomes) ficavam na pasta do executável no original.
// Como a instalação agora é em Program Files (sem permissão de escrita), eles são procurados primeiro
// em %APPDATA%\OracleTools2026 e depois na pasta do executável; gravações vão sempre para o %APPDATA%.
public static class AppFiles
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static string UserDataFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OracleTools2026");

    public static IReadOnlyList<string> SearchFolders { get; } = [UserDataFolder, AppContext.BaseDirectory];

    public static string? Find(string fileName) =>
        SearchFolders.Select(folder => Path.Combine(folder, fileName)).FirstOrDefault(File.Exists);

    public static string GetUserDataPath(string fileName) => Path.Combine(UserDataFolder, fileName);

    public static void OpenUserDataFolder()
    {
        Directory.CreateDirectory(UserDataFolder);
        Process.Start(new ProcessStartInfo(UserDataFolder) { UseShellExecute = true });
    }

    // O original gravava/lia em ANSI (Encoding.Default do .NET Framework); arquivos novos podem estar em UTF-8.
    public static async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);

        if (bytes is [0xEF, 0xBB, 0xBF, ..])
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        if (bytes is [0xFF, 0xFE, ..])
        {
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
        }

        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            var ansi = CodePagesEncodingProvider.Instance.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
                       ?? Encoding.Latin1;
            return ansi.GetString(bytes);
        }
    }

    public static async Task WriteAllTextAsync(string fileName, string text, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(UserDataFolder);
        await File.WriteAllTextAsync(GetUserDataPath(fileName), text, Encoding.UTF8, cancellationToken);
    }
}
