namespace OracleTools2026.Models;

public sealed record SessionKillResult(DatabaseSession Session, string? Error)
{
    public bool Succeeded => Error is null;
}
