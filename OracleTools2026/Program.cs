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
    private const string ExtractDdlTool = "EXTRACT_DDL_SOURCE";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.Message, "Oracle Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);

        using var host = CreateHost();
        var services = host.Services;

        TryConnectFromArguments(services.GetRequiredService<IOracleSession>(), args);

        var tool = args.Length > 0 ? args[0].Trim().ToUpperInvariant() : "";
        Form mainForm = tool switch
        {
            ExtractDdlTool => services.GetRequiredService<ExtractDdlForm>(),
            // Enquanto o menu principal não é migrado, a ferramenta padrão é o Extrair DDL.
            _ => services.GetRequiredService<ExtractDdlForm>()
        };

        Application.Run(mainForm);
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
        services.AddSingleton<DdlOutputService>();

        services.AddTransient<LoginForm>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<DatabaseInfoForm>();
        services.AddTransient<ExtractDdlForm>();
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
