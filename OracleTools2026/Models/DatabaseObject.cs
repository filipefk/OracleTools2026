namespace OracleTools2026.Models;

public sealed record DatabaseObject
{
    public required string Owner { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? LastDdlAt { get; init; }
    public string Status { get; init; } = "";
    public string ErrorMessage { get; init; } = "";
    public string LastModifiedBy { get; init; } = "";

    public string QualifiedName => $"{Owner}.{Name}";

    public string FileName => BuildFileName(Owner, Name, Type);

    public static string BuildFileName(string owner, string name, string type)
    {
        var extension = type.Trim().ToUpperInvariant() switch
        {
            "VIEW" => "vw",
            "TRIGGER" => "trg",
            "FUNCTION" => "fnc",
            "PROCEDURE" => "prc",
            "PACKAGE" or "PACKAGE BODY" => "pkb",
            "TYPE" or "TYPE BODY" => "typ",
            "JAVA SOURCE" => "jsp",
            _ => "sql"
        };

        return $"{owner.Trim().ToUpperInvariant()}.{name.Trim().ToUpperInvariant()}.{extension}";
    }
}
