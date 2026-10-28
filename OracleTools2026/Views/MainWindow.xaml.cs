using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.UI;

namespace OracleTools2026.Views;

// Janela principal: menu lateral com as ferramentas, que abrem como páginas e compartilham a conexão.
public sealed partial class MainWindow : Window
{
    private const string HomeTag = "home";

    private readonly IOracleSession _session = App.GetService<IOracleSession>();
    private readonly WindowService _windows = App.GetService<WindowService>();
    private readonly ToolHost _host;
    private bool _suppressNavigation;
    private bool _closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        WindowSetup.Apply(this, AppTitleBar, 1280, 800, 900, 560);
        _host = new ToolHost(this);

        BuildMenu();
        UpdateConnectionText();
        _session.ConnectionChanged += Session_ConnectionChanged;
        AppWindow.Closing += AppWindow_Closing;
        Closed += (_, _) => _session.ConnectionChanged -= Session_ConnectionChanged;

        // O item de configurações só existe depois que o template do NavigationView é aplicado.
        NavView.Loaded += (_, _) =>
        {
            if (NavView.SettingsItem is NavigationViewItem settingsItem)
            {
                settingsItem.Content = "Parâmetros";
            }
        };

        NavView.SelectedItem = NavView.MenuItems[0];
    }

    public void NavigateTo(ToolDefinition tool, string? argument = null)
    {
        _suppressNavigation = true;
        try
        {
            NavView.SelectedItem = FindMenuItem(tool.Command);
        }
        finally
        {
            _suppressNavigation = false;
        }

        Navigate(tool.PageType, argument);
    }

    public void OpenInNewWindow(ToolDefinition tool) => _windows.OpenToolWindow(tool);

    private void BuildMenu()
    {
        NavView.MenuItems.Add(new NavigationViewItem
        {
            Content = "Início",
            Tag = HomeTag,
            Icon = new FontIcon { Glyph = "\uE80F" }
        });

        foreach (var group in ToolCatalog.Tools.GroupBy(t => t.Group))
        {
            NavView.MenuItems.Add(new NavigationViewItemHeader { Content = group.Key });
            foreach (var tool in group)
            {
                var item = new NavigationViewItem
                {
                    Content = tool.Title,
                    Tag = tool.Command,
                    Icon = new FontIcon { Glyph = tool.Glyph }
                };
                ToolTipService.SetToolTip(item, $"{tool.Description}\n\nLinha de comando: OracleTools2026.exe {tool.Command}");

                var openInWindow = new MenuFlyoutItem { Text = "Abrir em nova janela", Icon = new FontIcon { Glyph = "\uE8A7" } };
                openInWindow.Click += (_, _) => OpenInNewWindow(tool);
                item.ContextFlyout = new MenuFlyout { Items = { openInWindow } };

                NavView.MenuItems.Add(item);
            }
        }
    }

    private NavigationViewItem? FindMenuItem(string tag) =>
        NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => tag.Equals(i.Tag as string, StringComparison.OrdinalIgnoreCase));

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressNavigation)
        {
            return;
        }

        if (args.IsSettingsSelected)
        {
            Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            if (tag == HomeTag)
            {
                Navigate(typeof(HomePage));
            }
            else if (ToolCatalog.Find(tag) is { } tool)
            {
                Navigate(tool.PageType);
            }
        }
    }

    private void Navigate(Type pageType, string? argument = null)
    {
        if (ContentFrame.CurrentSourcePageType == pageType && argument is null)
        {
            return;
        }

        ContentFrame.Navigate(pageType, argument, new EntranceNavigationTransitionInfo());
    }

    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args) => NavView.IsPaneOpen = !NavView.IsPaneOpen;

    private async void ConnectButton_Click(object sender, RoutedEventArgs e) => await _host.ShowLoginAsync();

    private async void ConnectAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await _host.ShowLoginAsync();
    }

    private void NewInstanceButton_Click(object sender, RoutedEventArgs e) => LaunchNewInstance();

    private void NewInstanceAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        LaunchNewInstance();
    }

    private async void LaunchNewInstance()
    {
        try
        {
            App.GetService<ToolLauncher>().LaunchNewInstance();
        }
        catch (Exception ex)
        {
            await DialogService.ShowMessageAsync(this, "Oracle Tools", $"Não foi possível abrir uma nova instância:\n{ex.Message}");
        }
    }

    private void Session_ConnectionChanged(object? sender, EventArgs e) =>
        DispatcherQueue.TryEnqueue(UpdateConnectionText);

    private void UpdateConnectionText()
    {
        var connected = _session.Current is { IsComplete: true };
        ConnectionText.Text = connected ? $"{_session.Current!.User}@{_session.Current.DataSource}" : "Não conectado";
        ConnectionIcon.Glyph = connected ? "\uE703" : "\uE7BA";
    }

    // Fechar a janela principal fecha também as ferramentas abertas em janelas próprias.
    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        var openWindows = _windows.ToolWindows.Count;
        if (_closeConfirmed || openWindows == 0)
        {
            return;
        }

        args.Cancel = true;
        if (await DialogService.AskAsync(this, "Oracle Tools", $"Há {openWindows} ferramenta(s) aberta(s) em outras janelas. Fechar todas e sair?"))
        {
            _closeConfirmed = true;
            _windows.CloseToolWindows();
            Close();
        }
    }
}
