using Microsoft.UI.Xaml;
using OracleTools2026.Models;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using System.ComponentModel;

namespace OracleTools2026.Views;

// Janela com uma única ferramenta: aberta pela linha de comando ou por "Abrir em nova janela".
public sealed partial class ToolWindow : Window
{
    private readonly TaskCompletionSource _loaded = new();
    private ToolViewModelBase? _viewModel;

    public ToolWindow(ToolDefinition tool, string? argument)
    {
        InitializeComponent();
        Tool = tool;
        Title = tool.Title;
        AppTitleBar.Title = tool.Title;
        WindowSetup.Apply(this, AppTitleBar, 1200, 720, 800, 450);

        // A página só fica disponível no Frame depois que a janela carrega.
        RootGrid.Loaded += (_, _) =>
        {
            AttachViewModel((ContentFrame.Content as ToolPage)?.ToolViewModel);
            _loaded.TrySetResult();
        };
        ContentFrame.Navigate(tool.PageType, argument);
        Closed += (_, _) => AttachViewModel(null);
    }

    public ToolDefinition Tool { get; }

    public async Task ShowMessageWhenReadyAsync(string title, string message)
    {
        await _loaded.Task;
        await DialogService.ShowMessageAsync(this, title, message);
    }

    // O título da janela mostra a ferramenta e o banco conectado, como no original.
    private void AttachViewModel(ToolViewModelBase? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        _viewModel = viewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateTitle();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToolViewModelBase.Title))
        {
            UpdateTitle();
        }
    }

    private void UpdateTitle()
    {
        if (_viewModel is null)
        {
            return;
        }

        Title = _viewModel.Title;
        AppTitleBar.Subtitle = _viewModel.ServerInfo;
    }
}
