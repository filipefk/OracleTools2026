namespace OracleTools2026.Models;

// MaxBytes nulo indica cota ilimitada (MAX_BYTES = -1 no dicionário de dados).
public sealed record TablespaceQuota(string Tablespace, long? MaxBytes, long UsedBytes)
{
    public bool IsUnlimited => MaxBytes is null;
}
