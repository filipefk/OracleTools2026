using System.Globalization;
using System.Text;

namespace OracleTools2026.Services;

// Associa logins (usuário do banco / usuário do SO) ao nome real da pessoa, usando os mesmos arquivos do original:
// ListaNomesAd.txt (cache "LOGIN<tab>NOME") e Lista_Usuarios2.txt (lista de usuários mantida manualmente).
public sealed class RealNameDirectory
{
    private const string CacheFileName = "ListaNomesAd.txt";
    private const string UserListFileName = "Lista_Usuarios2.txt";

    // Usuários genéricos que o original não procurava na lista de usuários.
    private static readonly HashSet<string> IgnoredLogins = new(["AESPROD", "GDP981", "GIP981", "PARALELO"], StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyDictionary<string, string>> ResolveAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        var names = await LoadCacheAsync(cancellationToken);
        var userList = await LoadUserListAsync(cancellationToken);

        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var added = false;
        foreach (var login in logins.Select(l => l.Trim()).Where(l => l.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (names.TryGetValue(login, out var name))
            {
                resolved[login] = name;
                continue;
            }

            name = FindInUserList(login, userList);
            if (name is null && login.LastIndexOf('\\') is var separator and >= 0)
            {
                // Usuário do SO no formato DOMINIO\login.
                name = FindInUserList(login[(separator + 1)..], userList);
            }

            if (name is not null)
            {
                names[login] = name;
                resolved[login] = name;
                added = true;
            }
        }

        if (added)
        {
            await SaveCacheAsync(names, cancellationToken);
        }

        return resolved;
    }

    public static bool AreSameName(string name1, string name2) =>
        NormalizeName(name1) == NormalizeName(name2);

    private static string NormalizeName(string name)
    {
        var decomposed = string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant()
            .Normalize(NormalizationForm.FormD);

        return string.Concat(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }

    private static string? FindInUserList(string login, IReadOnlyList<string[]> userList)
    {
        if (IgnoredLogins.Contains(login))
        {
            return null;
        }

        // O original pegava a primeira linha que contivesse o login; a coincidência exata na 1ª coluna tem prioridade.
        var columns = userList.FirstOrDefault(c => c[0].Equals(login, StringComparison.OrdinalIgnoreCase))
                      ?? userList.FirstOrDefault(c => c.Any(value => value.Contains(login, StringComparison.OrdinalIgnoreCase)));

        return columns is { Length: > 1 } ? columns[1].ToUpperInvariant() : null;
    }

    private static async Task<Dictionary<string, string>> LoadCacheAsync(CancellationToken cancellationToken)
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var columns in await ReadTabSeparatedAsync(CacheFileName, cancellationToken))
        {
            if (columns.Length > 1)
            {
                names.TryAdd(columns[0], columns[1]);
            }
        }

        return names;
    }

    private static Task<IReadOnlyList<string[]>> LoadUserListAsync(CancellationToken cancellationToken) =>
        ReadTabSeparatedAsync(UserListFileName, cancellationToken);

    private static async Task<IReadOnlyList<string[]>> ReadTabSeparatedAsync(string fileName, CancellationToken cancellationToken)
    {
        if (AppFiles.Find(fileName) is not { } path)
        {
            return [];
        }

        var text = await AppFiles.ReadAllTextAsync(path, cancellationToken);
        return text.ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(columns => columns.Length > 0)
            .ToList();
    }

    private static async Task SaveCacheAsync(Dictionary<string, string> names, CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        foreach (var (login, name) in names)
        {
            text.Append(login).Append('\t').Append(name).Append("\r\n");
        }

        try
        {
            await AppFiles.WriteAllTextAsync(CacheFileName, text.ToString(), cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // O cache só evita novas buscas; se não puder ser gravado, os nomes continuam sendo exibidos.
        }
    }
}
