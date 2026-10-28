namespace OracleTools2026.Models;

public sealed record UserScriptResult(string UserName, string Script, string? Error)
{
    public bool Succeeded => Error is null && !string.IsNullOrWhiteSpace(Script);

    public string FileName => $"{UserName.Trim().ToUpperInvariant()}.sql";

    public static UserScriptResult Failure(string userName, string error) => new(userName, "", error);
}
