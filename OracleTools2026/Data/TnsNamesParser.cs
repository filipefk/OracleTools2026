using System.Text;

namespace OracleTools2026.Data;

public static class TnsNamesParser
{
    public static IReadOnlyList<string> ReadAliases(string tnsNamesPath)
    {
        if (!File.Exists(tnsNamesPath))
        {
            return [];
        }

        var aliases = new List<string>();
        var token = new StringBuilder();
        var depth = 0;

        foreach (var rawLine in File.ReadLines(tnsNamesPath))
        {
            var commentStart = rawLine.IndexOf('#');
            var line = commentStart >= 0 ? rawLine[..commentStart] : rawLine;

            foreach (var ch in line)
            {
                switch (ch)
                {
                    case '(':
                        depth++;
                        break;
                    case ')':
                        depth = Math.Max(0, depth - 1);
                        break;
                    case '=' when depth == 0:
                        aliases.AddRange(token.ToString()
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(a => a.ToUpperInvariant()));
                        token.Clear();
                        break;
                    default:
                        if (depth == 0)
                        {
                            token.Append(ch);
                        }
                        break;
                }
            }

            if (depth == 0)
            {
                token.Append(' ');
            }
        }

        return aliases.Where(a => a.Length > 0).Distinct().Order().ToList();
    }

    public static string? GetTnsAdminFolder(string? tnsNamesPath)
    {
        var folder = string.IsNullOrWhiteSpace(tnsNamesPath) ? null : Path.GetDirectoryName(tnsNamesPath);
        return !string.IsNullOrEmpty(folder) && Directory.Exists(folder) ? folder : null;
    }
}
