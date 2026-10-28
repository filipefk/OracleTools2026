using System.Text;

namespace OracleTools2026.Formatting;

// Texto separado por TAB, com cabeçalho, usado ao copiar linhas das listas (mesmo formato do original).
public static class TabularText
{
    public static string Build(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        var text = new StringBuilder();
        text.AppendJoin('\t', headers.Select(h => h.Trim())).Append("\r\n");
        foreach (var row in rows)
        {
            text.AppendJoin('\t', row).Append("\r\n");
        }

        return text.ToString();
    }
}
