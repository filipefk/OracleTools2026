namespace OracleTools2026.Models;

// CustomFilter é a condição SQL livre do original ("Não colocar WHERE nem o primeiro AND").
public sealed record UserSearchCriteria(UserOwnerFilter OwnerFilter, string? Profile, string? Name, string? CustomFilter);
