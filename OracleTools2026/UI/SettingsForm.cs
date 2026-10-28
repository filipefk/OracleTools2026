using OracleTools2026.Settings;

namespace OracleTools2026.UI;

public partial class SettingsForm : Form
{
    private static readonly HashSet<string> ReadOnlySettings = new(StringComparer.OrdinalIgnoreCase)
    {
        SettingNames.SavedOracleConnections,
        "ConexoesFTPSalvas",
        "ConexoesSVNSalvas"
    };

    private readonly IAppSettings _settings;

    public SettingsForm(IAppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
    }

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        foreach (var name in _settings.GetNames())
        {
            if (_settings.GetValue(name) is null)
                continue;
            var index = settingsGrid.Rows.Add(name, SettingNames.GetFriendlyName(name), _settings.GetValue(name)!);
            if (ReadOnlySettings.Contains(name))
            {
                var row = settingsGrid.Rows[index];
                row.ReadOnly = true;
                row.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        settingsGrid.EndEdit();
        foreach (DataGridViewRow row in settingsGrid.Rows)
        {
            if (row.ReadOnly || row.Cells[nameColumn.Index].Value is not string name)
            {
                continue;
            }

            var newValue = row.Cells[valueColumn.Index].Value?.ToString() ?? "";
            if (newValue != (_settings.GetValue(name) ?? ""))
            {
                _settings.SetValue(name, newValue);
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
