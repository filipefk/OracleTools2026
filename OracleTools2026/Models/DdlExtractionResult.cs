namespace OracleTools2026.Models;

public sealed record DdlExtractionResult(string QualifiedName, string ObjectType, string Source, string? Error)
{
    public bool Succeeded => Error is null && !string.IsNullOrWhiteSpace(Source);

    public string Owner => QualifiedName.Split('.')[0];

    public string Name => QualifiedName.Split('.')[^1];

    public string FileName => DatabaseObject.BuildFileName(Owner, Name, ObjectType);

    public static DdlExtractionResult Failure(string qualifiedName, string error, string objectType = "") =>
        new(qualifiedName, objectType, "", error);
}
