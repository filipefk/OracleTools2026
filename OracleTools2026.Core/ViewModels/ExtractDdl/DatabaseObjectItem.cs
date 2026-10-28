using OracleTools2026.Formatting;
using OracleTools2026.Models;

namespace OracleTools2026.ViewModels.ExtractDdl;

public sealed class DatabaseObjectItem(DatabaseObject databaseObject) : CheckableItem
{
    public DatabaseObject Object { get; } = databaseObject;

    public string Owner => Object.Owner;
    public string Name => Object.Name;
    public string Type => Object.Type;
    public DateTime? CreatedAt => Object.CreatedAt;
    public string CreatedAtText => DisplayFormat.Date(Object.CreatedAt);
    public DateTime? LastDdlAt => Object.LastDdlAt;
    public string LastDdlAtText => DisplayFormat.Date(Object.LastDdlAt);
    public string Status => Object.Status;
    public string ErrorText => Object.ErrorMessage.ReplaceLineEndings(" | ");
    public string QualifiedName => Object.QualifiedName;

    public bool IsInvalid => Object.Status != "VALID";

    public bool HasError => Object.ErrorMessage.Length > 0;
}
