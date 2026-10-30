using System.Text.RegularExpressions;

namespace OracleTools2026.Models;

// Endereço de um banco sem tnsnames.ora: host, porta e service name (ou SID).
public sealed partial record ServerAddress(string Host, int Port, string ServiceName, bool IsSid)
{
    public const int DefaultPort = 1521;

    // EZConnect não aceita SID; nesse caso é usado o descritor completo.
    public string ToDataSource() => IsSid
        ? $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SID={ServiceName})))"
        : $"{Host}:{Port}/{ServiceName}";

    public override string ToString() => IsSid ? $"{Host}:{Port}:{ServiceName}" : $"{Host}:{Port}/{ServiceName}";

    // Reconhece EZConnect simples ([//]host[:porta]/serviço) e descritores com um único endereço TCP.
    public static bool TryParse(string? dataSource, out ServerAddress? address)
    {
        address = null;
        var text = dataSource?.Trim() ?? "";
        if (text.Length == 0)
        {
            return false;
        }

        if (EzConnectRegex().Match(text) is { Success: true } ez)
        {
            var port = ez.Groups["port"].Success ? int.Parse(ez.Groups["port"].Value) : DefaultPort;
            address = new ServerAddress(ez.Groups["host"].Value, port, ez.Groups["service"].Value, false);
            return IsValidPort(port);
        }

        if (!text.StartsWith('('))
        {
            return false;
        }

        var compact = WhitespaceRegex().Replace(text, "");
        if (AddressRegex().Count(compact) != 1)
        {
            return false;
        }

        var protocol = ValueOf(compact, "PROTOCOL");
        var host = ValueOf(compact, "HOST");
        var portText = ValueOf(compact, "PORT");
        var service = ValueOf(compact, "SERVICE_NAME");
        var sid = ValueOf(compact, "SID");
        if ((protocol is not null && !protocol.Equals("TCP", StringComparison.OrdinalIgnoreCase)) ||
            host is null || (service ?? sid) is null)
        {
            return false;
        }

        var descriptorPort = DefaultPort;
        if (portText is not null && !int.TryParse(portText, out descriptorPort))
        {
            return false;
        }

        address = new ServerAddress(host, descriptorPort, (service ?? sid)!, service is null);
        return IsValidPort(descriptorPort);
    }

    public static bool IsValidPort(int port) => port is > 0 and <= 65535;

    // EZConnect ou descritor (qualquer formato), em oposição a um alias do tnsnames.ora.
    public static bool IsDirect(string? dataSource) =>
        dataSource?.Trim() is { Length: > 0 } text && (text.StartsWith('(') || text.Contains('/') || text.Contains(':'));

    private static string? ValueOf(string descriptor, string key) =>
        Regex.Match(descriptor, $@"\({key}=([^()]+)\)", RegexOptions.IgnoreCase) is { Success: true } match
            ? match.Groups[1].Value
            : null;

    [GeneratedRegex(@"^(?://)?(?<host>[^:/\s()]+)(?::(?<port>\d{1,5}))?/(?<service>[^:/\s()]+)$")]
    private static partial Regex EzConnectRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\(ADDRESS=", RegexOptions.IgnoreCase)]
    private static partial Regex AddressRegex();
}
