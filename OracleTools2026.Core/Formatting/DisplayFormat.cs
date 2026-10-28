using System.Globalization;

namespace OracleTools2026.Formatting;

// Formatos usados nas listas (os mesmos do original).
public static class DisplayFormat
{
    public const string DateFormat = "dd/MM/yy HH:mm:ss";
    private const string TimeFormat = "hh':'mm':'ss";

    public static string Date(DateTime? value) =>
        value?.ToString(DateFormat, CultureInfo.InvariantCulture) ?? "";

    public static string Duration(TimeSpan? value) => value switch
    {
        null => "",
        { Days: > 0 } duration => $"{duration.Days}d {duration.ToString(TimeFormat, CultureInfo.InvariantCulture)}",
        { } duration => duration.ToString(TimeFormat, CultureInfo.InvariantCulture)
    };

    // Valores de consultas genéricas: nulos aparecem como NULL, como no original.
    public static string Value(object? value) => value switch
    {
        null or DBNull => "NULL",
        DateTime date => Date(date),
        DateTimeOffset date => Date(date.DateTime),
        TimeSpan duration => Duration(duration),
        byte[] bytes => Convert.ToHexString(bytes),
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? ""
    };

    public static string Bytes(long bytes)
    {
        string[] units = ["bytes", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} bytes" : $"{size.ToString("0.##", CultureInfo.CurrentCulture)} {units[unit]}";
    }

    public static string Time(DateTime value) => value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    public static string SingleLine(string text, string separator = " ") =>
        string.Join(separator, text.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()));
}
