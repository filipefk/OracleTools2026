using Microsoft.UI.Xaml.Controls;
using OracleTools2026.Models;
using OracleTools2026.Presentation;
using System.Text;

namespace OracleTools2026.Views.Dialogs;

public sealed partial class DatabaseInfoDialog : ContentDialog
{
    private readonly IReadOnlyList<DatabaseProperty> _properties;

    public DatabaseInfoDialog(IReadOnlyList<DatabaseProperty> properties)
    {
        _properties = properties;
        InitializeComponent();
        GroupedProperties.Source = properties
            .GroupBy(p => p.Group)
            .Select(g => new DatabasePropertyGroup(g.Key, g))
            .ToList();
    }

    // Copia as linhas selecionadas (ou todas) no formato "Grupo / Nome: Valor", com uma linha em branco entre os grupos.
    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        var selected = PropertiesList.SelectedItems.OfType<DatabaseProperty>().ToList();
        var items = selected.Count > 0 ? selected : _properties;

        var text = new StringBuilder();
        foreach (var group in items.GroupBy(p => p.Group))
        {
            if (text.Length > 0)
            {
                text.AppendLine();
            }

            text.AppendLine(group.Key);
            foreach (var property in group)
            {
                text.AppendLine($"{property.Name}: {property.Value}");
            }
        }

        if (text.Length > 0)
        {
            App.GetService<IClipboardService>().SetText(text.ToString());
        }
    }
}
