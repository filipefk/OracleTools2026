using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OracleTools2026.ViewModels.Preferences;

namespace OracleTools2026.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }

    // Relê o registro a cada exibição (as ferramentas também gravam preferências).
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.ReloadCommand.Execute(null);
    }
}
