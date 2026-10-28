using OracleTools2026.Models;
using System.DirectoryServices;
using System.Text;

namespace OracleTools2026.Services;

// Consulta de usuários no Active Directory via LDAP. A API é síncrona: as telas chamam dentro de Task.Run.
public sealed class ActiveDirectoryService
{
    private const int MaxResults = 200;

    // Bits de userAccountControl.
    private const int AccountDisabledFlag = 0x2;
    private const int PasswordNeverExpiresFlag = 0x10000;

    private static readonly string[] Properties =
    [
        "samaccountname", "displayname", "description", "mail", "department", "title", "useraccountcontrol",
        "pwdlastset", "lockouttime", "lastlogontimestamp", "accountexpires", "memberof"
    ];

    public void Test(DirectoryConnection connection)
    {
        using var entry = CreateEntry(connection);
        using var searcher = new DirectorySearcher(entry, "(sAMAccountName=TESTEDECONEXAO)", ["samaccountname"]);
        searcher.FindOne();
    }

    // O padrão aceita * e % como curinga e é comparado com o nome de exibição e o login.
    public IReadOnlyList<DirectoryUser> FindUsers(DirectoryConnection connection, string pattern, CancellationToken cancellationToken = default)
    {
        var value = EscapeFilterValue(pattern.Trim().Replace('%', '*'));
        var filter = $"(&(objectCategory=person)(objectClass=user)(|(displayName={value})(sAMAccountName={value})))";

        using var entry = CreateEntry(connection);
        using var searcher = new DirectorySearcher(entry, filter, Properties)
        {
            SizeLimit = MaxResults,
            PageSize = MaxResults
        };
        searcher.Sort = new SortOption("sAMAccountName", SortDirection.Ascending);

        using var results = searcher.FindAll();
        var users = new List<DirectoryUser>();
        foreach (SearchResult result in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            users.Add(ToUser(result));
        }

        return users;
    }

    private static DirectoryEntry CreateEntry(DirectoryConnection connection)
    {
        var entry = connection.Path.Length == 0 ? new DirectoryEntry() : new DirectoryEntry(connection.Path);
        if (!connection.UsesWindowsCredentials)
        {
            entry.Username = connection.User.Trim();
            entry.Password = connection.Password;
        }

        entry.AuthenticationType = AuthenticationTypes.Secure;
        return entry;
    }

    private static DirectoryUser ToUser(SearchResult result)
    {
        var accountControl = (int)GetLong(result, "useraccountcontrol");
        var passwordLastSet = GetLong(result, "pwdlastset");

        return new DirectoryUser(
            GetString(result, "samaccountname").ToUpperInvariant(),
            GetString(result, "displayname"),
            GetString(result, "description"),
            GetString(result, "mail"),
            GetString(result, "department"),
            GetString(result, "title"),
            (accountControl & AccountDisabledFlag) != 0,
            GetLong(result, "lockouttime") > 0,
            (accountControl & PasswordNeverExpiresFlag) != 0,
            passwordLastSet is 0 or -1,
            ToDate(GetLong(result, "lastlogontimestamp")),
            ToDate(passwordLastSet),
            ToDate(GetLong(result, "accountexpires")),
            result.Properties["memberof"].Cast<object>()
                .Select(dn => GetCommonName(dn.ToString() ?? ""))
                .Where(name => name.Length > 0)
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList());
    }

    private static string GetString(SearchResult result, string property) =>
        result.Properties[property] is { Count: > 0 } values ? values[0]?.ToString() ?? "" : "";

    private static long GetLong(SearchResult result, string property) =>
        result.Properties[property] is { Count: > 0 } values ? Convert.ToInt64(values[0]) : 0;

    // Datas do AD são FILETIME; 0 e Int64.MaxValue significam "nunca".
    private static DateTime? ToDate(long fileTime) =>
        fileTime is > 0 and < long.MaxValue ? DateTime.FromFileTimeUtc(fileTime).ToLocalTime() : null;

    // "CN=Nome do Grupo,OU=...,DC=..." → "Nome do Grupo" (vírgulas no nome vêm escapadas com "\").
    private static string GetCommonName(string distinguishedName)
    {
        if (!distinguishedName.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            return distinguishedName;
        }

        var name = new StringBuilder();
        for (var i = 3; i < distinguishedName.Length; i++)
        {
            var c = distinguishedName[i];
            if (c == '\\' && i + 1 < distinguishedName.Length)
            {
                name.Append(distinguishedName[++i]);
            }
            else if (c == ',')
            {
                break;
            }
            else
            {
                name.Append(c);
            }
        }

        return name.ToString();
    }

    // RFC 4515, mantendo o * como curinga.
    private static string EscapeFilterValue(string value)
    {
        var escaped = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            escaped.Append(c switch
            {
                '\\' => @"\5c",
                '(' => @"\28",
                ')' => @"\29",
                '\0' => @"\00",
                _ => c.ToString()
            });
        }

        return escaped.ToString();
    }
}
