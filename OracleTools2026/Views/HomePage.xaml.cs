using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.UI;
using System.Reflection;

namespace OracleTools2026.Views;

// Página inicial: as ferramentas agrupadas em cartões e a conexão atual.
public sealed partial class HomePage : Page
{
    private readonly IOracleSession _session = App.GetService<IOracleSession>();
    private readonly WindowService _windows = App.GetService<WindowService>();

    public HomePage()
    {
        InitializeComponent();

        GroupsControl.ItemsSource = ToolCatalog.Tools
            .GroupBy(t => t.Group)
            .Select(g => new ToolGroup(g.Key, g.ToList()))
            .ToList();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Versão {version?.ToString(3)}";

        Loaded += (_, _) =>
        {
            UpdateConnection();
            _session.ConnectionChanged += Session_ConnectionChanged;
        };
        Unloaded += (_, _) => _session.ConnectionChanged -= Session_ConnectionChanged;
    }

    public static string GetToolTip(string command, string description) =>
        $"{description}\n\nLinha de comando: OracleTools2026.exe {command}";

    private void ToolGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ToolDefinition tool)
        {
            _windows.MainWindow?.NavigateTo(tool);
        }
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_windows.FindWindow(XamlRoot) is { } window)
        {
            await new ToolHost(window).ShowLoginAsync();
        }
    }

    private void Session_ConnectionChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(UpdateConnection);

    private void UpdateConnection()
    {
        if (_session.Current is { IsComplete: true } current)
        {
            ConnectionInfoBar.Severity = InfoBarSeverity.Success;
            ConnectionInfoBar.Title = "Conectado";
            ConnectionInfoBar.Message = $"{current.User}@{current.DataSource}";
        }
        else
        {
            ConnectionInfoBar.Severity = InfoBarSeverity.Informational;
            ConnectionInfoBar.Title = "Não conectado";
            ConnectionInfoBar.Message = "As ferramentas pedem o login quando precisarem do banco.";
        }
    }
}
