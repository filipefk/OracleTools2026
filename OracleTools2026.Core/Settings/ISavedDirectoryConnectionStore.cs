using OracleTools2026.Models;

namespace OracleTools2026.Settings;

public interface ISavedDirectoryConnectionStore
{
    IReadOnlyList<SavedDirectoryConnection> GetAll();
    SavedDirectoryConnection? Find(DirectoryConnection connection);
    void Save(SavedDirectoryConnection connection);
    void Remove(string alias);
}
