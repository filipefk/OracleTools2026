namespace OracleTools2026.UI;

public static class AppIcon
{
    private static readonly Lazy<Icon?> SharedIcon = new(() =>
        Environment.ProcessPath is { } path ? Icon.ExtractAssociatedIcon(path) : null);

    public static void ApplyTo(Form form)
    {
        if (SharedIcon.Value is { } icon)
        {
            form.Icon = icon;
        }
    }
}
