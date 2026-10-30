using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace OracleTools2026.Models;

public sealed partial record ConnectionInfo(string User, string Password, string DataSource, DbaPrivilege Privilege = DbaPrivilege.Normal)
{
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(User) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(DataSource);

    // Usuário com o privilégio no formato do sqlplus ("SYS AS SYSDBA"), usado nas conexões salvas e no último login.
    public string StoredUser => JoinUser(User, Privilege);

    public string DataSourceDisplay
    {
        get
        {
            var text = DataSource.Trim();
            if (ServerAddress.TryParse(text, out var address))
            {
                return address!.ToString();
            }

            return ServerAddress.IsDirect(text)
                ? WhitespaceRegex().Replace(text, "")
                : text.ToUpperInvariant();
        }
    }

    public string DisplayName => Privilege == DbaPrivilege.Normal
        ? $"{User}@{DataSourceDisplay}"
        : $"{User}@{DataSourceDisplay} ({PrivilegeName(Privilege)})";

    public string BuildConnectionString()
    {
        var builder = new OracleConnectionStringBuilder
        {
            UserID = User,
            Password = Password,
            DataSource = DataSource
        };

        if (Privilege != DbaPrivilege.Normal)
        {
            builder.DBAPrivilege = PrivilegeName(Privilege);
        }

        return builder.ConnectionString;
    }

    public static string PrivilegeName(DbaPrivilege privilege) => privilege switch
    {
        DbaPrivilege.SysDba => "SYSDBA",
        DbaPrivilege.SysOper => "SYSOPER",
        _ => "Normal"
    };

    public static (string User, DbaPrivilege Privilege) SplitUser(string? user)
    {
        var text = user?.Trim() ?? "";
        if (UserWithPrivilegeRegex().Match(text) is not { Success: true } match)
        {
            return (text, DbaPrivilege.Normal);
        }

        var privilege = match.Groups["privilege"].Value.Equals("SYSDBA", StringComparison.OrdinalIgnoreCase)
            ? DbaPrivilege.SysDba
            : DbaPrivilege.SysOper;
        return (match.Groups["user"].Value, privilege);
    }

    public static string JoinUser(string user, DbaPrivilege privilege) =>
        privilege == DbaPrivilege.Normal ? user : $"{user} AS {PrivilegeName(privilege)}";

    [GeneratedRegex(@"^(?<user>\S+)\s+AS\s+(?<privilege>SYSDBA|SYSOPER)$", RegexOptions.IgnoreCase)]
    private static partial Regex UserWithPrivilegeRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
