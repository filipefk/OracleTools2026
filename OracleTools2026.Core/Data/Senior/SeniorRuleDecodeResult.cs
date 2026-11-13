namespace OracleTools2026.Data.Senior;

public sealed record SeniorRuleDecodeResult(string? Source, string? Error)
{
    public static SeniorRuleDecodeResult Failure(string error) => new(null, error);
}
