using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OracleTools2026.Presentation;

namespace OracleTools2026.UI;

// Funções usadas em x:Bind.
public static class BindingHelpers
{
    public static bool Not(bool value) => !value;

    public static Visibility VisibleWhen(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility CollapsedWhen(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility VisibleWhenNotEmpty(string? value) =>
        string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

    public static InfoBarSeverity Severity(NoticeKind kind) => kind switch
    {
        NoticeKind.Success => InfoBarSeverity.Success,
        NoticeKind.Warning => InfoBarSeverity.Warning,
        NoticeKind.Error => InfoBarSeverity.Error,
        _ => InfoBarSeverity.Informational
    };
}
