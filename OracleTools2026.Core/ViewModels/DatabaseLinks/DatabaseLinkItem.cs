using CommunityToolkit.Mvvm.ComponentModel;
using OracleTools2026.Formatting;
using OracleTools2026.Models;

namespace OracleTools2026.ViewModels.DatabaseLinks;

public sealed partial class DatabaseLinkItem : ObservableObject
{
    public const string PendingText = "Aguardando teste...";

    public DatabaseLinkItem(DatabaseLink link)
    {
        Link = link;
        TestText = "";
    }

    public DatabaseLink Link { get; }

    public DateTime? CreatedAt => Link.CreatedAt;

    public string CreatedAtText => DisplayFormat.Date(Link.CreatedAt);

    public string Owner => Link.Owner;

    public string Name => Link.Name;

    public string UserName => Link.UserName;

    public string Host => Link.Host;

    [ObservableProperty]
    public partial string TestText { get; private set; }

    [ObservableProperty]
    public partial DatabaseLinkTestState TestState { get; private set; }

    public void SetPending() => SetTest(DatabaseLinkTestState.Pending, PendingText);

    public void ClearPending()
    {
        if (TestState == DatabaseLinkTestState.Pending)
        {
            SetTest(DatabaseLinkTestState.None, "");
        }
    }

    public void SetResult(DatabaseLinkTestResult result)
    {
        if (result.Succeeded)
        {
            SetTest(DatabaseLinkTestState.Ok, "TESTE OK");
        }
        else
        {
            SetTest(DatabaseLinkTestState.Error, $"ERRO: {result.Error?.ReplaceLineEndings(" ")}");
        }
    }

    private void SetTest(DatabaseLinkTestState state, string text)
    {
        TestText = text;
        TestState = state;
    }
}
