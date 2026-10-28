using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.DatabaseLinks;
using OracleTools2026.ViewModels.Dialogs;
using OracleTools2026.ViewModels.ExtractDdl;
using OracleTools2026.ViewModels.ExtractUser;
using OracleTools2026.ViewModels.LockMonitor;
using OracleTools2026.ViewModels.Preferences;
using OracleTools2026.ViewModels.SchedulerJobs;
using OracleTools2026.ViewModels.Sessions;
using OracleTools2026.ViewModels.UserDetails;

namespace OracleTools2026;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();
        _host = CreateHost();

        UnhandledException += App_UnhandledException;
        TaskScheduler.UnobservedTaskException += (_, e) => e.SetObserved();
    }

    public static IServiceProvider Services => ((App)Current)._host.Services;

    public static T GetService<T>() where T : notnull => Services.GetRequiredService<T>();

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var windows = GetService<WindowService>();

        // Formato: OracleTools2026.exe <FERRAMENTA> [usuario senha alias_tns] [argumento]
        // Sem argumento, ou com uma ferramenta desconhecida, abre o menu (como no original).
        var commandLine = Environment.GetCommandLineArgs().Skip(1).ToArray();
        var tool = commandLine.Length > 0 ? ToolCatalog.Find(commandLine[0]) : null;
        if (tool is null)
        {
            windows.ShowMainWindow();
            return;
        }

        // Pela linha de comando a ferramenta roda isolada, em um processo com a sua própria conexão.
        // O 5º argumento (ex.: nome do usuário em DETALHES_USER) vai para a ferramenta, como no original.
        var connectionError = await TryConnectFromArgumentsAsync(GetService<IOracleSession>(), commandLine);
        var window = windows.OpenToolWindow(tool, commandLine.Length > 4 ? commandLine[4] : null);
        if (connectionError is not null)
        {
            await window.ShowMessageWhenReadyAsync("Oracle Tools", connectionError);
        }
    }

    private static async Task<string?> TryConnectFromArgumentsAsync(IOracleSession session, string[] args)
    {
        if (args.Length < 4)
        {
            return null;
        }

        var connection = new ConnectionInfo(args[1].Trim(), args[2].Trim(), args[3].Trim());
        if (!connection.IsComplete)
        {
            return null;
        }

        try
        {
            await Task.Run(() => session.ConnectAsync(connection));
            return null;
        }
        catch (Exception ex)
        {
            return $"Não foi possível conectar em {connection.User}@{connection.DataSource}:\n{ex.Message}";
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        _ = GetService<WindowService>().ShowErrorInActiveWindowAsync(e.Exception?.Message ?? e.Message);
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;

        services.AddSingleton<IAppSettings, RegistryAppSettings>();
        services.AddSingleton<ISavedConnectionStore, SavedConnectionStore>();
        services.AddSingleton<ISavedDirectoryConnectionStore, SavedDirectoryConnectionStore>();
        services.AddSingleton<IOracleSession, OracleSession>();
        services.AddSingleton<DatabaseObjectRepository>();
        services.AddSingleton<DatabaseUserRepository>();
        services.AddSingleton<UserScriptGenerator>();
        services.AddSingleton<DdlExtractor>();
        services.AddSingleton<InvalidObjectRecompiler>();
        services.AddSingleton<ScriptRunner>();
        services.AddSingleton<SessionRepository>();
        services.AddSingleton<SchedulerJobRepository>();
        services.AddSingleton<DatabaseLinkRepository>();
        services.AddSingleton<DdlOutputService>();
        services.AddSingleton<LockScriptStore>();
        services.AddSingleton<RealNameDirectory>();
        services.AddSingleton<ActiveDirectoryService>();
        services.AddSingleton<ToolLauncher>();

        // Interface
        services.AddSingleton<WindowService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IToolNavigator, ToolNavigator>();
        services.AddSingleton<ToolServices>();

        // ViewModels: cada página (ou janela avulsa) tem a sua instância.
        services.AddTransient<ExtractDdlViewModel>();
        services.AddTransient<ExtractUserViewModel>();
        services.AddTransient<UserDetailsViewModel>();
        services.AddTransient<LockMonitorViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<SchedulerJobsViewModel>();
        services.AddTransient<DatabaseLinksViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DirectoryLoginViewModel>();
        services.AddTransient<SettingsViewModel>();

        return builder.Build();
    }
}
