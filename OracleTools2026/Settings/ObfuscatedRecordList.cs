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
            plain = LegacyCipher.Decrypt(encrypted).Replace(Filler, "");
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
            var fillerCount = Random.Shared.Next(5, 11);
            for (var i = 0; i < fillerCount; i++)
            {
                block = block.Insert(Random.Shared.Next(1, block.Length), Filler);
            }

            builder.Append(block);
        }

        return LegacyCipher.Encrypt(builder.ToString());
    }
}
