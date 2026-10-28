using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OracleTools2026.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OracleTools2026.ViewModels.Preferences;

// Parâmetros gravados no registro (mesmas chaves do original). As conexões salvas são encriptadas e
// mostradas somente para leitura.
public sealed partial class SettingsViewModel : ObservableObject
{
    private static readonly HashSet<string> ReadOnlySettings = new(StringComparer.OrdinalIgnoreCase)
    {
        SettingNames.SavedOracleConnections,
        SettingNames.SavedDirectoryConnections,
        "ConexoesFTPSalvas",
        "ConexoesSVNSalvas"
    };

    private readonly IAppSettings _settings;

    public SettingsViewModel(IAppSettings settings)
    {
        _settings = settings;
        StatusMessage = "";
    }

    public ObservableCollection<SettingItem> Items { get; } = [];

    public bool HasChanges => Items.Any(i => i.IsModified);

    [ObservableProperty]
    public partial string StatusMessage { get; private set; }

    [RelayCommand]
    private void Reload()
    {
        foreach (var item in Items)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        Items.Clear();
        foreach (var name in _settings.GetNames())
        {
            if (_settings.GetValue(name) is not { } value)
            {
                continue;
            }

            var friendlyName = SettingNames.GetFriendlyName(name);
            var item = SettingNames.IsFlag(name)
                ? SettingItem.CreateFlag(name, friendlyName, _settings.GetFlag(name))
                : new SettingItem(name, friendlyName, value, ReadOnlySettings.Contains(name));
            item.PropertyChanged += Item_PropertyChanged;
            Items.Add(item);
        }

        StatusMessage = "";
        NotifyChanges();
    }

    [RelayCommand(CanExecute = nameof(HasChanges))]
    private void Save()
    {
        var saved = 0;
        foreach (var item in Items.Where(i => i.IsEditable && i.IsModified))
        {
            if (item.IsFlag)
            {
                _settings.SetFlag(item.Name, item.IsOn);
            }
            else
            {
                _settings.SetValue(item.Name, item.Value);
            }

            item.AcceptChanges();
            saved++;
        }

        StatusMessage = $"{saved} parâmetro(s) gravado(s).";
        NotifyChanges();
    }

    [RelayCommand(CanExecute = nameof(HasChanges))]
    private void Discard() => Reload();

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingItem.IsModified))
        {
            NotifyChanges();
        }
    }

    private void NotifyChanges()
    {
        OnPropertyChanged(nameof(HasChanges));
        SaveCommand.NotifyCanExecuteChanged();
        DiscardCommand.NotifyCanExecuteChanged();
    }
}
