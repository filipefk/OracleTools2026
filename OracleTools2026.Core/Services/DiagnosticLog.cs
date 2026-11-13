using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace OracleTools2026.Services;

// Log de diagnóstico em %APPDATA%\OracleTools2026\diagnostico.log. Registra as exceções não tratadas e,
// com a pilha completa, toda exceção .NET no momento em que é lançada (first chance): algumas falhas do
// WinUI fecham o processo sem passar pelo Application.UnhandledException, e só esse registro mostra a
// origem. Cada linha é gravada na hora, porque o processo pode ser encerrado logo em seguida.
public static class DiagnosticLog
{
    public const string FileName = "diagnostico.log";

    private const long MaxFileSize = 5 * 1024 * 1024;
    private const int MaxFirstChanceEntries = 1000;

    private static readonly Lock WriteLock = new();
    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    [ThreadStatic]
    private static bool _writing;

    private static int _firstChanceEntries;
    private static bool _installed;

    public static string FilePath => AppFiles.GetUserDataPath(FileName);

    public static void Install()
    {
        if (_installed)
        {
            return;
        }

        _installed = true;
        RotateIfLarge();
        WriteSessionHeader();

        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Write($"Exceção não tratada (AppDomain, encerrando: {e.IsTerminating})", e.ExceptionObject as Exception,
                e.ExceptionObject as Exception is null ? e.ExceptionObject?.ToString() : null);
        TaskScheduler.UnobservedTaskException += (_, e) => Write("Exceção não observada em Task", e.Exception);
    }

    public static void Write(string title, Exception? exception = null, string? details = null)
    {
        var text = new StringBuilder()
            .Append(Timestamp()).Append(' ').Append(title).Append(ThreadInfo()).Append('\n');
        if (details is not null)
        {
            text.Append(details.ReplaceLineEndings("\n")).Append('\n');
        }

        if (exception is not null)
        {
            text.Append(exception.ToString().ReplaceLineEndings("\n")).Append('\n');
            if (exception.HResult != 0)
            {
                text.Append("HRESULT: 0x").Append(exception.HResult.ToString("X8", CultureInfo.InvariantCulture)).Append('\n');
            }
        }

        Append(text.Append('\n').ToString());
    }

    // A pilha de uma exceção recém-lançada só tem o método que lançou; a pilha atual mostra quem chamou.
    private static void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        if (_writing || Interlocked.Increment(ref _firstChanceEntries) > MaxFirstChanceEntries)
        {
            return;
        }

        var exception = e.Exception;
        var text = new StringBuilder()
            .Append(Timestamp()).Append(" [first chance] ").Append(exception.GetType().FullName)
            .Append(" (HRESULT 0x").Append(exception.HResult.ToString("X8", CultureInfo.InvariantCulture)).Append(')')
            .Append(ThreadInfo()).Append('\n')
            .Append(exception.Message.ReplaceLineEndings("\n")).Append('\n');

        if (exception is not OperationCanceledException)
        {
            text.Append(new StackTrace(1, fNeedFileInfo: false).ToString().ReplaceLineEndings("\n"));
        }

        if (_firstChanceEntries == MaxFirstChanceEntries)
        {
            text.Append($"(limite de {MaxFirstChanceEntries} exceções first chance nesta execução; as próximas não são registradas)\n");
        }

        Append(text.Append('\n').ToString());
    }

    private static void WriteSessionHeader()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? assembly?.GetName().Version?.ToString() ?? "?";
        Append($"""
            ==== {Timestamp()} Início: OracleTools2026 {version}, processo {Environment.ProcessId}, {Environment.OSVersion}, .NET {Environment.Version}
            Linha de comando: {string.Join(' ', Environment.GetCommandLineArgs().Skip(1).Take(1))}

            """.ReplaceLineEndings("\n"));
    }

    // Uma geração anterior é mantida em diagnostico.old.log.
    private static void RotateIfLarge()
    {
        try
        {
            var file = new FileInfo(FilePath);
            if (file.Exists && file.Length > MaxFileSize)
            {
                file.CopyTo(Path.ChangeExtension(FilePath, ".old.log"), overwrite: true);
                file.Delete();
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void Append(string text)
    {
        if (_writing)
        {
            return;
        }

        _writing = true;
        try
        {
            lock (WriteLock)
            {
                Directory.CreateDirectory(AppFiles.UserDataFolder);
                File.AppendAllText(FilePath, text, FileEncoding);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        finally
        {
            _writing = false;
        }
    }

    private static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    private static string ThreadInfo() =>
        $" [thread {Environment.CurrentManagedThreadId}{(Thread.CurrentThread.Name is { Length: > 0 } name ? " " + name : "")}]";
}
