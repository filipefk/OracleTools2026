namespace OracleTools2026.Settings;

public interface IAppSettings
{
    string? TnsNamesPath { get; set; }
    string? LastLoginUser { get; set; }
    string? LastLoginDataSource { get; set; }
    string? DefaultOutputFolder { get; set; }
    bool IncludeHeaderOnDdlExtraction { get; set; }
    bool InlineCompileErrorsOnDdlExtraction { get; set; }
    string? UserSearchFilter { get; set; }

    string? GetValue(string name);
    void SetValue(string name, string? value);
    IReadOnlyList<string> GetNames();
}
