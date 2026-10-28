using OracleTools2026.Presentation;
using Windows.ApplicationModel.DataTransfer;

namespace OracleTools2026.UI;

public sealed class ClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
        package.SetText(text);
        Clipboard.SetContent(package);

        try
        {
            // Mantém o texto na área de transferência depois que o programa for fechado.
            Clipboard.Flush();
        }
        catch (Exception)
        {
            // Outro programa pode estar com a área de transferência aberta; o texto já foi copiado.
        }
    }
}
