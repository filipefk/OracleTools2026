using CommunityToolkit.Mvvm.ComponentModel;

namespace OracleTools2026.ViewModels;

// Linha de lista que pode ser marcada (checkbox na primeira coluna).
public abstract partial class CheckableItem : ObservableObject
{
    [ObservableProperty]
    public partial bool IsChecked { get; set; }
}
