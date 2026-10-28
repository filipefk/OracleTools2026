using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OracleTools2026.Presentation;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;

namespace OracleTools2026.Views;

// Base das páginas de ferramenta: associa o ViewModel à janela em que a página está, ativa a ferramenta
// na primeira exibição, repassa o argumento de abertura e encerra a ferramenta quando a janela fecha.
// A página fica em cache na navegação para não perder o estado ao trocar de ferramenta.
public partial class ToolPage : Page
{
    private bool _attached;

    public ToolPage()
    {
        NavigationCacheMode = NavigationCacheMode.Required;
        Loaded += ToolPage_Loaded;
    }

    public virtual ToolViewModelBase? ToolViewModel => null;

    protected ToolHost? Host { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string { Length: > 0 } argument && ToolViewModel is IToolArgumentReceiver receiver)
        {
            receiver.SetToolArgument(argument.Trim());
        }
    }

    private async void ToolPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (ToolViewModel is not { } viewModel)
        {
            return;
        }

        if (!_attached)
        {
            if (App.GetService<WindowService>().FindWindow(XamlRoot) is not { } window)
            {
                return;
            }

            _attached = true;
            Host = new ToolHost(window);
            viewModel.AttachHost(Host);
            window.Closed += (_, _) => viewModel.Deactivate();
        }

        await viewModel.ActivateAsync();
    }
}
