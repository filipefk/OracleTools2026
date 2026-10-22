using OracleTools2026.Models;

namespace OracleTools2026.UI;

// Dados de um nó da árvore de detalhes do usuário. Name é o usuário, a role ou o profile a que o nó se refere.
public sealed record UserTreeNodeInfo(UserTreeNodeKind Kind, string Name = "", ObjectPrivilegeGrant? Grant = null)
{
    public static UserTreeNodeInfo Info { get; } = new(UserTreeNodeKind.Info);
}
