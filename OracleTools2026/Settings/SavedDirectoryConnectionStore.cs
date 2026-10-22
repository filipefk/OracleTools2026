using OracleTools2026.Models;

namespace OracleTools2026.Settings;

// Conexões do Active Directory no formato do original (chave ConexoesAD): "apelido\nusuario\nsenha\ndominio\nporta\n".
public sealed class SavedDirectoryConnectionStore(IAppSettings settings) : ISavedDirectoryConnectionStore
{
    public IReadOnlyList<SavedDirectoryConnection> GetAll() =>
        ObfuscatedRecordList.Decode(settings.GetValue(SettingNames.SavedDirectoryConnections), 5)
            .Select(r => new SavedDirectoryConnection(r[0], new DirectoryConnection(r[3], r[4], r[1], r[2])))
            .ToList();

    public SavedDirectoryConnection? Find(DirectoryConnection connection) =>
        GetAll().FirstOrDefault(c =>
            string.Equals(c.Connection.Domain, connection.Domain, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Connection.Port, connection.Port, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Connection.User, connection.User, StringComparison.OrdinalIgnoreCase));

    public void Save(SavedDirectoryConnection connection)
    {
        var connections = GetAll().Where(c => c.Alias != connection.Alias).ToList();
        connections.Insert(0, connection);
        Persist(connections);
    }

    public void Remove(string alias) =>
        Persist(GetAll().Where(c => c.Alias != alias).ToList());

    private void Persist(IReadOnlyList<SavedDirectoryConnection> connections) =>
        settings.SetValue(SettingNames.SavedDirectoryConnections, ObfuscatedRecordList.Encode(
            connections.Select<SavedDirectoryConnection, IReadOnlyList<string>>(c =>
                [c.Alias, c.Connection.User, c.HasPassword ? c.Connection.Password : "", c.Connection.Domain, c.Connection.Port])));
}
