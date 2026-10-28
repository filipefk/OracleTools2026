using OracleTools2026.Models;

namespace OracleTools2026.Views.Dialogs;

public sealed class DatabasePropertyGroup(string name, IEnumerable<DatabaseProperty> properties) : List<DatabaseProperty>(properties)
{
    public string Name { get; } = name;
}
