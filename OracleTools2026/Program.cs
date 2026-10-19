using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using OracleTools2026.UI;

namespace OracleTools2026;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.Message, "Oracle Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);

        using var host = CreateHost();
        var services = host.Services;

        // Sem argumento, ou com uma ferramenta desconhecida, abre o menu inicial (como no original).
        var tool = args.Length > 0 ? ToolCatalog.Find(args[0]) : null;
        if (tool is null)
        {
            Application.Run(services.GetRequiredService<MainForm>());
            return;
        }

        // Pela linha de comando a ferramenta roda isolada, em um processo com a sua própria conexão.
        TryConnectFromArguments(services.GetRequiredService<IOracleSession>(), args);
        Application.Run(services.GetRequiredService<ToolWindowManager>().Create(tool));
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;

        services.AddSingleton<IAppSettings, RegistryAppSettings>();
        services.AddSingleton<ISavedConnectionStore, SavedConnectionStore>();
        services.AddSingleton<IOracleSession, OracleSession>();
        services.AddSingleton<DatabaseObjectRepository>();
        services.AddSingleton<DdlExtractor>();
        services.AddSingleton<InvalidObjectRecompiler>();
        services.AddSingleton<ScriptRunner>();
        services.AddSingleton<SessionRepository>();
        services.AddSingleton<SchedulerJobRepository>();
        services.AddSingleton<DatabaseLinkRepository>();
        services.AddSingleton<DdlOutputService>();
        services.AddSingleton<LockScriptStore>();
        services.AddSingleton<RealNameDirectory>();
        services.AddSingleton<ToolLauncher>();
        services.AddSingleton<ToolWindowManager>();
        services.AddSingleton<DatabaseToolDependencies>();

        // As telas de ferramenta (ToolCatalog) são criadas pelo ToolWindowManager e não precisam ser registradas.
        services.AddTransient<MainForm>();
        services.AddTransient<LoginForm>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<DatabaseInfoForm>();
        services.AddSingleton<Func<LoginForm>>(sp => () => sp.GetRequiredService<LoginForm>());
        services.AddSingleton<Func<SettingsForm>>(sp => () => sp.GetRequiredService<SettingsForm>());
        services.AddSingleton<Func<DatabaseInfoForm>>(sp => () => sp.GetRequiredService<DatabaseInfoForm>());

        return builder.Build();
    }

    // Formato: OracleTools2026.exe <FERRAMENTA> [usuario senha alias_tns]
    private static void TryConnectFromArguments(IOracleSession session, string[] args)
    {
        if (args.Length < 4)
        {
            return;
        }

        var connection = new ConnectionInfo(args[1].Trim(), args[2].Trim(), args[3].Trim());
        if (!connection.IsComplete)
        {
            return;
        }

        try
        {
            Task.Run(() => session.ConnectAsync(connection)).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível conectar em {connection.User}@{connection.DataSource}:\n{ex.Message}",
                "Oracle Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
