namespace OracleTools2026.Models;

// Conexão LDAP com o Active Directory. Usuário em branco usa as credenciais do Windows; domínio em
// branco usa o domínio do computador.
public sealed record DirectoryConnection(string Domain, string Port, string User, string Password)
{
    public bool UsesWindowsCredentials => string.IsNullOrWhiteSpace(User);

    public string Path => (Domain.Trim(), Port.Trim()) switch
    {
        ("", _) => "",
        (var domain, "") => $"LDAP://{domain}",
        (var domain, var port) => $"LDAP://{domain}:{port}"
    };

    public override string ToString() =>
        $"{(UsesWindowsCredentials ? "conta do Windows" : User)}@{(Domain.Trim().Length > 0 ? Domain.Trim() : "domínio atual")}";
}
