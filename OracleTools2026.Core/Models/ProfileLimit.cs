namespace OracleTools2026.Models;

// InheritedFromDefault indica que o limite é DEFAULT no profile e o valor mostrado vem do profile DEFAULT.
public sealed record ProfileLimit(string ResourceType, string ResourceName, string Limit, bool InheritedFromDefault);
