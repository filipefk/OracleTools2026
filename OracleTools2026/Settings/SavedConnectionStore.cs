using OracleTools2026.Models;
using System.Text;

namespace OracleTools2026.Settings;

// Formato compatível com o OracleTools original: blocos "apelido\nusuario\nsenha\nbanco\n" em ordem inversa,
// com uma string de ofuscação inserida em posições aleatórias, tudo encriptado com LegacyCipher.
public sealed class SavedConnectionStore(IAppSettings settings) : ISavedConnectionStore
{
    private static readonly string Filler =
        $"[{Environment.ProcessorCount}{Environment.MachineName}{Environment.ProcessorCount}]";

    public IReadOnlyList<SavedConnection> GetAll()
    {
        var encrypted = settings.GetValue(SettingNames.SavedOracleConnections);
        if (string.IsNullOrWhiteSpace(encrypted))
        {
            return [];
        }

        string plain;
        try
        {
            plain = LegacyCipher.Decrypt(encrypted).Replace(Filler, "");
        }
        catch (Exception ex) when (ex is FormatException or System.Security.Cryptography.CryptographicException)
        {
            return [];
        }

        var parts = plain.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var connections = new List<SavedConnection>();
        for (var i = 0; i + 3 < parts.Length; i += 4)
        {
            var password = string.IsNullOrWhiteSpace(parts[i + 2]) ? "" : parts[i + 2];
            connections.Add(new SavedConnection(parts[i], parts[i + 1], password, parts[i + 3]));
        }

        return connections;
    }

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

    private void Persist(IReadOnlyList<SavedConnection> connections)
    {
        var builder = new StringBuilder();
        foreach (var connection in connections)
        {
            var password = connection.HasPassword ? connection.Password : " ";
            var block = $"{connection.Alias}\n{connection.User}\n{password}\n{connection.DataSource}\n";
            var fillerCount = Random.Shared.Next(5, 11);
            for (var i = 0; i < fillerCount; i++)
            {
                var position = Random.Shared.Next(1, block.Length);
                block = block.Insert(position, Filler);
            }

            builder.Append(block);
        }

        settings.SetValue(SettingNames.SavedOracleConnections, LegacyCipher.Encrypt(builder.ToString()));
    }
}
