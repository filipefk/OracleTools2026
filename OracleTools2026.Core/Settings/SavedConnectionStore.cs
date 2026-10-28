using OracleTools2026.Models;

namespace OracleTools2026.Settings;

// Formato compatível com o OracleTools original: blocos "apelido\nusuario\nsenha\nbanco\n" (ObfuscatedRecordList).
public sealed class SavedConnectionStore(IAppSettings settings) : ISavedConnectionStore
{
    public IReadOnlyList<SavedConnection> GetAll() =>
        ObfuscatedRecordList.Decode(settings.GetValue(SettingNames.SavedOracleConnections), 4)
            .Select(r => new SavedConnection(r[0], r[1], r[2], r[3]))
            .ToList();

    public SavedConnection? FindByUserAndDataSource(string user, string dataSource) =>
        GetAll().FirstOrDefault(c =>
            string.Equals(c.User, user, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.DataSource, dataSource, StringComparison.OrdinalIgnoreCase));

    public void Save(SavedConnection connection)
    {
        var connections = GetAll().Where(c => c.Alias != connection.Alias).ToList();
        connections.Insert(0, connection);
        Persist(connections);
    }

    public void Remove(string alias) =>
        Persist(GetAll().Where(c => c.Alias != alias).ToList());

    private void Persist(IReadOnlyList<SavedConnection> connections) =>
        settings.SetValue(SettingNames.SavedOracleConnections, ObfuscatedRecordList.Encode(
            connections.Select<SavedConnection, IReadOnlyList<string>>(c =>
                [c.Alias, c.User, c.HasPassword ? c.Password : "", c.DataSource])));
}
