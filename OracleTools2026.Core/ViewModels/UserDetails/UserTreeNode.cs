using CommunityToolkit.Mvvm.ComponentModel;
using OracleTools2026.Models;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels.UserDetails;

// Nó da árvore de permissões. Nós "sob demanda" (IsLazy) carregam os filhos ao serem expandidos.
public sealed partial class UserTreeNode : ObservableObject
{
    public UserTreeNode(string text, UserTreeNodeKind kind = UserTreeNodeKind.Info, string name = "",
        ObjectPrivilegeGrant? grant = null, bool isLazy = false, bool isMuted = false)
    {
        Text = text;
        Kind = kind;
        Name = name;
        Grant = grant;
        IsLazy = isLazy;
        IsMuted = isMuted;
    }

    public string Text { get; }

    public UserTreeNodeKind Kind { get; }

    public string Name { get; }

    public ObjectPrivilegeGrant? Grant { get; }

    // Marcadores como "(Nenhuma role)" aparecem esmaecidos.
    public bool IsMuted { get; }

    public ObservableCollection<UserTreeNode> Children { get; } = [];

    [ObservableProperty]
    public partial bool IsLazy { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public UserTreeNode Add(UserTreeNode child)
    {
        Children.Add(child);
        return child;
    }

    public UserTreeNode AddInfo(string text) => Add(new UserTreeNode(text));

    public void AddLazy(string text, UserTreeNodeKind kind, string name) =>
        Add(new UserTreeNode(text, kind, name, isLazy: true));

    public void AddEmptyMarker(string text)
    {
        if (Children.Count == 0)
        {
            Add(new UserTreeNode($"({text})", isMuted: true));
        }
    }

    public IEnumerable<UserTreeNode> FindLazyNodes()
    {
        if (IsLazy)
        {
            yield return this;
            yield break;
        }

        foreach (var lazy in Children.SelectMany(c => c.FindLazyNodes()))
        {
            yield return lazy;
        }
    }

    public void ExpandAll()
    {
        IsExpanded = true;
        foreach (var child in Children)
        {
            child.ExpandAll();
        }
    }

    public override string ToString() => Text;
}
