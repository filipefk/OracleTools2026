using OracleTools2026.Data;
using OracleTools2026.Models;
using OracleTools2026.Settings;

namespace OracleTools2026.UI;

public partial class LoginForm : Form
{
    private readonly IOracleSession _session;
    private readonly IAppSettings _settings;
    private readonly ISavedConnectionStore _savedConnections;

    public LoginForm(IOracleSession session, IAppSettings settings, ISavedConnectionStore savedConnections)
    {
        _session = session;
        _settings = settings;
        _savedConnections = savedConnections;
        InitializeComponent();
    }

    private void LoginForm_Load(object? sender, EventArgs e)
    {
        var current = _session.Current;
        userTextBox.Text = current?.User ?? _settings.LastLoginUser ?? "";
        dataSourceComboBox.Text = current?.DataSource ?? _settings.LastLoginDataSource ?? "";
        LoadDataSources(promptIfMissing: false);

        ActiveControl = userTextBox.Text.Length == 0 ? userTextBox : passwordTextBox;
    }

    private void DataSourceComboBox_DropDown(object? sender, EventArgs e)
    {
        if (dataSourceComboBox.Items.Count > 0 || LoadDataSources(promptIfMissing: false))
        {
            return;
        }

        // Abrir um diálogo modal dentro do evento DropDown deixa a lista aberta vazia e sem foco;
        // fecha a lista, pergunta o arquivo e reabre já preenchida.
        BeginInvoke(() =>
        {
            dataSourceComboBox.DroppedDown = false;
            if (LoadDataSources(promptIfMissing: true))
            {
                dataSourceComboBox.Focus();
                dataSourceComboBox.DroppedDown = true;
            }
        });
    }

    private void LocateTnsNamesButton_Click(object? sender, EventArgs e)
    {
        if (SelectTnsNamesFile() is not null)
        {
            LoadDataSources(promptIfMissing: false);
        }
    }

    private void SaveConnectionCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        savePasswordCheckBox.Enabled = saveConnectionCheckBox.Checked;
        if (!saveConnectionCheckBox.Checked)
        {
            savePasswordCheckBox.Checked = false;
        }
    }

    private void SavedConnectionsButton_Click(object? sender, EventArgs e)
    {
        savedConnectionsMenu.Items.Clear();
        var connections = _savedConnections.GetAll();
        if (connections.Count == 0)
        {
            savedConnectionsMenu.Items.Add(new ToolStripMenuItem("Nenhuma conexão salva") { Enabled = false });
        }
        else
        {
            foreach (var connection in connections)
            {
                savedConnectionsMenu.Items.Add(connection.Alias, null, (_, _) => ApplySavedConnection(connection));
            }

            var removeMenu = new ToolStripMenuItem("Excluir");
            foreach (var connection in connections)
            {
                removeMenu.DropDownItems.Add(connection.Alias, null, (_, _) => RemoveSavedConnection(connection));
            }

            savedConnectionsMenu.Items.Add(new ToolStripSeparator());
            savedConnectionsMenu.Items.Add(removeMenu);
        }

        savedConnectionsMenu.Show(savedConnectionsButton, new Point(0, savedConnectionsButton.Height));
    }

    private async void OkButton_Click(object? sender, EventArgs e)
    {
        var connection = new ConnectionInfo(userTextBox.Text.Trim(), passwordTextBox.Text, dataSourceComboBox.Text.Trim());
        if (!connection.IsComplete)
        {
            MessageBox.Show(this, "Informe usuário, senha e banco.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        UseWaitCursor = true;
        mainLayout.Enabled = false;
        try
        {
            await Task.Run(() => _session.ConnectAsync(connection));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Erro ao conectar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        finally
        {
            UseWaitCursor = false;
            mainLayout.Enabled = true;
        }

        if (saveConnectionCheckBox.Checked && !TrySaveConnection(connection))
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private bool TrySaveConnection(ConnectionInfo connection)
    {
        var alias = _savedConnections.FindByUserAndDataSource(connection.User, connection.DataSource)?.Alias
                    ?? InputDialog.Show(this, "Apelido da conexão", "Informe o apelido da conexão:", $"{connection.User}@{connection.DataSource}");

        if (string.IsNullOrWhiteSpace(alias))
        {
            return false;
        }

        var password = savePasswordCheckBox.Checked ? connection.Password : "";
        _savedConnections.Save(new SavedConnection(alias, connection.User, password, connection.DataSource));
        return true;
    }

    private void ApplySavedConnection(SavedConnection connection)
    {
        userTextBox.Text = connection.User;
        passwordTextBox.Text = connection.Password;
        dataSourceComboBox.Text = connection.DataSource;
        saveConnectionCheckBox.Checked = true;
        savePasswordCheckBox.Checked = connection.HasPassword;
        ActiveControl = connection.HasPassword ? okButton : passwordTextBox;
    }

    private void RemoveSavedConnection(SavedConnection connection)
    {
        var answer = MessageBox.Show(this, $"Tem certeza que deseja apagar a conexão \"{connection.Alias}\"?",
            "Apagar conexão salva", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            _savedConnections.Remove(connection.Alias);
        }
    }

    private bool LoadDataSources(bool promptIfMissing)
    {
        var path = _settings.TnsNamesPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            path = promptIfMissing ? SelectTnsNamesFile() : null;
        }

        if (path is null)
        {
            return false;
        }

        var text = dataSourceComboBox.Text;
        dataSourceComboBox.BeginUpdate();
        dataSourceComboBox.Items.Clear();
        dataSourceComboBox.Items.AddRange(TnsNamesParser.ReadAliases(path).Cast<object>().ToArray());
        dataSourceComboBox.EndUpdate();
        dataSourceComboBox.Text = text;
        return dataSourceComboBox.Items.Count > 0;
    }

    private string? SelectTnsNamesFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Informe o caminho do arquivo tnsnames.ora",
            Filter = "TNS Names|tnsnames.ora|Todos os arquivos|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        _settings.TnsNamesPath = dialog.FileName;
        return dialog.FileName;
    }
}
