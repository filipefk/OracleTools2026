using Microsoft.Win32;

namespace OracleTools2026.Settings;

public sealed class RegistryAppSettings : IAppSettings
{
    private const string RegistryPath = @"SOFTWARE\ORACLE_Tools\Parametros";

    public string? TnsNamesPath
    {
        get => GetValue(SettingNames.TnsNamesPath);
        set => SetValue(SettingNames.TnsNamesPath, value);
    }

    public string? LastLoginUser
    {
        get => GetValue(SettingNames.LastLoginUser);
        set => SetValue(SettingNames.LastLoginUser, value);
    }

    public string? LastLoginDataSource
    {
        get => GetValue(SettingNames.LastLoginDataSource);
        set => SetValue(SettingNames.LastLoginDataSource, value);
    }

    public string? DefaultOutputFolder
    {
        get => GetValue(SettingNames.DefaultOutputFolder);
        set => SetValue(SettingNames.DefaultOutputFolder, value);
    }

    public bool IncludeHeaderOnDdlExtraction
    {
        get => GetFlag(SettingNames.IncludeHeaderOnDdlExtraction);
        set => SetFlag(SettingNames.IncludeHeaderOnDdlExtraction, value);
    }

    public bool InlineCompileErrorsOnDdlExtraction
    {
        get => GetFlag(SettingNames.InlineCompileErrorsOnDdlExtraction);
        set => SetFlag(SettingNames.InlineCompileErrorsOnDdlExtraction, value);
    }

    public string? GetValue(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        return key?.GetValue(name)?.ToString();
    }

    public void SetValue(string name, string? value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
        key.SetValue(name, value ?? "");
    }

    public IReadOnlyList<string> GetNames()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        return key?.GetValueNames().Order(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
    }

    private bool GetFlag(string name)
    {
        var value = GetValue(name)?.Trim().ToUpperInvariant();
        switch (value)
        {
            case "1" or "TRUE" or "SIM":
                return true;
            case "0" or "FALSE" or "NÃO" or "NAO":
                return false;
            default:
                SetFlag(name, true);
                return true;
        }
    }

    private void SetFlag(string name, bool value) => SetValue(name, value ? "1" : "0");
}
