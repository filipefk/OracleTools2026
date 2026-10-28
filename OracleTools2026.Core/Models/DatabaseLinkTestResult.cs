namespace OracleTools2026.Models;

public sealed record DatabaseLinkTestResult(DatabaseLink Link, string? Error)
{
    public bool Succeeded => Error is null;
}
