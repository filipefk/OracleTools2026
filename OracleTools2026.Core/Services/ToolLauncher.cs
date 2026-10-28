using System.Diagnostics;

namespace OracleTools2026.Services;

// As ferramentas abertas pelo menu compartilham a conexão do processo. Para usar outra conexão ao mesmo
// tempo, abre-se uma nova instância do executável, que tem a sua própria conexão.
public sealed class ToolLauncher
{
    public void LaunchNewInstance()
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("Não foi possível identificar o executável.");
        Process.Start(new ProcessStartInfo(executable) { UseShellExecute = false });
    }
}
