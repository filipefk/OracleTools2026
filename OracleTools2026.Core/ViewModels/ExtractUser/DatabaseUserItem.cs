using OracleTools2026.Formatting;
using OracleTools2026.Models;

namespace OracleTools2026.ViewModels.ExtractUser;

public sealed class DatabaseUserItem(DatabaseUser user) : CheckableItem
{
    public DatabaseUser User { get; } = user;

    public string Name => User.Name;
    public DateTime? CreatedAt => User.CreatedAt;
    public string CreatedAtText => DisplayFormat.Date(User.CreatedAt);
    public string Profile => User.Profile;
    public string Status => User.Status;
    public DateTime? LockDate => User.LockDate;
    public string LockDateText => DisplayFormat.Date(User.LockDate);
    public DateTime? ExpiryDate => User.ExpiryDate;
    public string ExpiryDateText => DisplayFormat.Date(User.ExpiryDate);
    public DateTime? LastLogin => User.LastLogin;
    public string LastLoginText => DisplayFormat.Date(User.LastLogin);

    public bool IsLocked => User.Status.Contains("LOCKED", StringComparison.Ordinal);
}
