using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OracleTools2026.ViewModels;

namespace OracleTools2026.Controls;

public sealed partial class ToolStatusBar : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(ToolViewModelBase), typeof(ToolStatusBar), new PropertyMetadata(null));

    public static readonly DependencyProperty ExtraTextProperty = DependencyProperty.Register(
        nameof(ExtraText), typeof(string), typeof(ToolStatusBar), new PropertyMetadata(""));

    public ToolStatusBar()
    {
        InitializeComponent();
    }

    public ToolViewModelBase? ViewModel
    {
        get => (ToolViewModelBase?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    // Texto à direita do status (ex.: quantidade de itens listados).
    public string ExtraText
    {
        get => (string)GetValue(ExtraTextProperty);
        set => SetValue(ExtraTextProperty, value);
    }
}
