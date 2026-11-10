using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Presentation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OracleTools2026.Views.Dialogs;

public sealed partial class AboutDialog : ContentDialog
{
    private const string SourceUrl = "https://github.com/filipefk/OracleTools2026";

    private readonly string _product;
    private readonly IReadOnlyList<AboutProperty> _properties;

    public AboutDialog()
    {
        InitializeComponent();

        var assembly = typeof(App).Assembly;
        _product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Oracle Tools 2026";
        _properties =
        [
            new("Versão", InformationalVersion(assembly) ?? "desconhecida"),
            new("Arquitetura", RuntimeInformation.ProcessArchitecture.ToString()),
            new(".NET", RuntimeInformation.FrameworkDescription),
            new("Windows App SDK", WindowsAppSdkVersion()),
            new("Driver Oracle", InformationalVersion(typeof(OracleConnection).Assembly) ?? "desconhecida"),
            new("Windows", Environment.OSVersion.Version.ToString()),
            new("Instalado em", Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory))
        ];

        ProductText.Text = _product;
        DescriptionText.Text = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;
        DescriptionText.Visibility = DescriptionText.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        InfoList.ItemsSource = _properties;
        SourceLink.Content = SourceUrl;
        SourceLink.NavigateUri = new Uri(SourceUrl);
        ToolTipService.SetToolTip(SourceLink, SourceUrl);

        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        CopyrightText.Text = copyright ?? string.Empty;
        CopyrightText.Visibility = string.IsNullOrEmpty(copyright) ? Visibility.Collapsed : Visibility.Visible;
    }

    // Copiar não fecha o diálogo; o próprio botão indica o resultado (outro diálogo não pode ser aberto aqui).
    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        var text = new StringBuilder();
        text.AppendLine(_product);
        foreach (var property in _properties)
        {
            text.AppendLine($"{property.Name}: {property.Value}");
        }

        text.AppendLine($"Fontes: {SourceUrl}");

        try
        {
            App.GetService<IClipboardService>().SetText(text.ToString());
            PrimaryButtonText = "Copiado";
        }
        catch (Exception)
        {
            PrimaryButtonText = "Não foi possível copiar";
        }
    }

    // O SDK pode acrescentar "+<hash do commit>" à versão informativa.
    private static string? InformationalVersion(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString();
        var suffix = version?.IndexOf('+', StringComparison.Ordinal) ?? -1;
        return suffix >= 0 ? version![..suffix] : version;
    }

    // Constantes geradas pelo pacote (WindowsAppSdkIncludeVersionInfo): como o Windows App SDK vai junto
    // do programa (self-contained), é a versão em uso.
    private static string WindowsAppSdkVersion() =>
        $"{Microsoft.WindowsAppSDK.Release.Major}.{Microsoft.WindowsAppSDK.Release.Minor}.{Microsoft.WindowsAppSDK.Release.Patch}"
        + Microsoft.WindowsAppSDK.Release.FormattedVersionTag;
}
