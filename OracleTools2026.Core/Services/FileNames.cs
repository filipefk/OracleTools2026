namespace OracleTools2026.Services;

public static class FileNames
{
    // Troca os caracteres que o Windows não aceita em nomes de arquivo por "_".
    public static string ToSafeFileName(string fileName) =>
        string.Concat(fileName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
