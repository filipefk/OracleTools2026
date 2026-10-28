using Microsoft.UI.Xaml.Controls;

namespace OracleTools2026.UI;

// Monta os menus de contexto das listas, que mudam conforme a linha clicada (como no original).
public static class MenuBuilder
{
    public static MenuFlyoutItem Add(this MenuFlyout menu, string text, string? glyph, Action action)
    {
        var item = new MenuFlyoutItem { Text = text };
        if (glyph is not null)
        {
            item.Icon = new FontIcon { Glyph = glyph };
        }

        item.Click += (_, _) => action();
        menu.Items.Add(item);
        return item;
    }

    public static void AddSeparator(this MenuFlyout menu)
    {
        if (menu.Items.Count > 0 && menu.Items[^1] is not MenuFlyoutSeparator)
        {
            menu.Items.Add(new MenuFlyoutSeparator());
        }
    }
}
