using System.Security.Cryptography;
using System.Text;

namespace OracleTools2026.Settings;

// Mantém compatibilidade com os dados encriptados pelo OracleTools original (RijndaelManaged/CBC).
public static class LegacyCipher
{
    private const string PasswordHash = "HD6hhRDf";
    private const string SaltKey = "LKKS@g46";
    private const string IvKey = "UJ63@#dsgeuUIIkg";
    private const int Iterations = 1000;

    public static string Encrypt(string plainText)
    {
        using var aes = CreateAes();
        var cipherBytes = aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), Iv, PaddingMode.Zeros);
        return Convert.ToBase64String(cipherBytes);
    }

    public static string Decrypt(string cipherText)
    {
        using var aes = CreateAes();
        var plainBytes = aes.DecryptCbc(Convert.FromBase64String(cipherText), Iv, PaddingMode.None);
        return Encoding.UTF8.GetString(plainBytes).TrimEnd('\0');
    }

    private static byte[] Iv => Encoding.ASCII.GetBytes(IvKey);

    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(PasswordHash, Encoding.ASCII.GetBytes(SaltKey), Iterations, HashAlgorithmName.SHA1, 32);
        return aes;
    }
}
