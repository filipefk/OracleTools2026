using OracleTools2026.Formatting;
using OracleTools2026.Models;
using OracleTools2026.Services;

namespace OracleTools2026.ViewModels.Sessions;

public sealed class SessionItem : CheckableItem
{
    public SessionItem(DatabaseSession session, IReadOnlyDictionary<string, string> realNames)
    {
        Session = session;
        UserRealName = realNames.GetValueOrDefault(session.UserName, "");
        OsUserRealName = realNames.GetValueOrDefault(session.OsUser, "");

        // Usuário do banco e usuário do SO pertencem a pessoas diferentes.
        IsDifferentPerson = UserRealName.Length > 0 && OsUserRealName.Length > 0 &&
                            !RealNameDirectory.AreSameName(UserRealName, OsUserRealName);
    }

    public DatabaseSession Session { get; }

    public int InstanceId => Session.InstanceId;
    public long Sid => Session.Sid;
    public long SerialNumber => Session.SerialNumber;
    public string UserName => Session.UserName;
    public string UserRealName { get; }
    public string OsUser => Session.OsUser;
    public string OsUserRealName { get; }
    public string Machine => Session.Machine;
    public string Status => Session.Status;
    public string Program => Session.Program;
    public string Type => Session.Type;
    public string Module => Session.Module;
    public DateTime? LogonTime => Session.LogonTime;
    public string LogonTimeText => DisplayFormat.Date(Session.LogonTime);
    public string State => Session.State;

    public bool IsDifferentPerson { get; }
}
