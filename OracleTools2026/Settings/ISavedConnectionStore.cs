using OracleTools2026.Models;

namespace OracleTools2026.Settings;

public interface ISavedConnectionStore
{
    IReadOnlyList<SavedConnection> GetAll();
    SavedConnection? FindByUserAndDataSource(string user, string dataSource);
    void Save(SavedConnection connection);
    void Remove(string alias);
}
