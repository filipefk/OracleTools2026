using System.Text;

namespace OracleTools2026.Settings;

// Formato das conexões salvas do OracleTools original: cada registro é uma sequência de campos terminados
// por "\n", com uma string de ofuscação inserida em posições aleatórias, tudo encriptado com LegacyCipher.
internal static class ObfuscatedRecordList
{
    private static readonly string Filler =
        $"[{Environment.ProcessorCount}{Environment.MachineName}{Environment.ProcessorCount}]";

    public static IReadOnlyList<string[]> Decode(string? encrypted, int fieldCount)
    {
        if (string.IsNullOrWhiteSpace(encrypted))
        {
            return [];
        }

        string plain;
        try
        {
            plain = RemoveFiller(LegacyCipher.Decrypt(encrypted));
        }
        catch (Exception ex) when (ex is FormatException or System.Security.Cryptography.CryptographicException)
        {
            return [];
        }

        return plain.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(field => string.IsNullOrWhiteSpace(field) ? "" : field)
            .Chunk(fieldCount)
            .Where(record => record.Length == fieldCount)
            .ToList();
    }

    public static string Encode(IEnumerable<IReadOnlyList<string>> records)
    {
        var builder = new StringBuilder();
        foreach (var record in records)
        {
            // Campos vazios são gravados como " " (como a senha não salva no original), pois a leitura
            // descarta linhas vazias.
            var block = string.Concat(record.Select(field => $"{(string.IsNullOrEmpty(field) ? " " : field)}\n"));
            // As posições são sorteadas no texto original, para uma string de ofuscação nunca cair dentro de outra.
            var positions = Enumerable.Range(0, Random.Shared.Next(5, 11))
                .Select(_ => Random.Shared.Next(1, block.Length))
                .Order();
            var start = 0;
            foreach (var position in positions)
            {
                builder.Append(block, start, position - start).Append(Filler);
                start = position;
            }

            builder.Append(block, start, block.Length - start);
        }

        return LegacyCipher.Encrypt(builder.ToString());
    }

    // Repete a remoção porque gravações antigas podem ter uma string de ofuscação inserida dentro de outra.
    private static string RemoveFiller(string text)
    {
        string previous;
        do
        {
            previous = text;
            text = text.Replace(Filler, "");
        }
        while (text.Length != previous.Length);

        return text;
    }
}
