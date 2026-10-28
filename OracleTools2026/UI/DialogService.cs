using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.CompilerServices;

namespace OracleTools2026.UI;

// Só pode haver um ContentDialog aberto por janela; os pedidos de cada janela são atendidos em fila
// (ex.: uma ferramenta em segundo plano mostrando um erro enquanto outra pede o login).
public static class DialogService
{
    private static readonly ConditionalWeakTable<Window, SemaphoreSlim> Gates = new();

    public static async Task<ContentDialogResult> ShowAsync(Window window, ContentDialog dialog)
    {
        var gate = Gates.GetValue(window, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            dialog.XamlRoot = window.Content.XamlRoot;
            dialog.Style ??= (Style)Application.Current.Resources["DefaultContentDialogStyle"];
            if (window.Content is FrameworkElement root)
            {
                dialog.RequestedTheme = root.ActualTheme;
            }

            return await dialog.ShowAsync();
        }
        finally
        {
            gate.Release();
        }
    }

    public static async Task<bool> AskAsync(Window window, string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = CreateMessageContent(message),
            PrimaryButtonText = "Sim",
            CloseButtonText = "Não",
            DefaultButton = ContentDialogButton.Primary
        };

        return await ShowAsync(window, dialog) == ContentDialogResult.Primary;
    }

    public static async Task ShowMessageAsync(Window window, string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = CreateMessageContent(message),
            CloseButtonText = "Fechar",
            DefaultButton = ContentDialogButton.Close
        };

        await ShowAsync(window, dialog);
    }

    private static ScrollViewer CreateMessageContent(string message) => new()
    {
        MaxHeight = 420,
        Content = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        }
    };
}
