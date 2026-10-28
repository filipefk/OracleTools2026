using CommunityToolkit.Mvvm.ComponentModel;

namespace OracleTools2026.ViewModels.Preferences;

public sealed partial class SettingItem : ObservableObject
{
    private const string FlagOn = "1";
    private const string FlagOff = "0";

    public SettingItem(string name, string friendlyName, string value, bool isReadOnly, bool isFlag = false)
    {
        Name = name;
        FriendlyName = friendlyName;
        IsFlag = isFlag;
        OriginalValue = value;
        Value = value;
        IsReadOnly = isReadOnly;
    }

    public static SettingItem CreateFlag(string name, string friendlyName, bool value) =>
        new(name, friendlyName, value ? FlagOn : FlagOff, isReadOnly: false, isFlag: true);

    public string Name { get; }

    public string FriendlyName { get; }

    public string OriginalValue { get; private set; }

    public bool IsReadOnly { get; }

    public bool IsEditable => !IsReadOnly;

    public bool IsFlag { get; }

    public bool IsText => !IsFlag;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModified))]
    [NotifyPropertyChangedFor(nameof(IsOn))]
    public partial string Value { get; set; }

    public bool IsOn
    {
        get => Value == FlagOn;
        set => Value = value ? FlagOn : FlagOff;
    }

    public bool IsModified => Value != OriginalValue;

    public void AcceptChanges()
    {
        OriginalValue = Value;
        OnPropertyChanged(nameof(IsModified));
    }
}
