using OracleTools2026.Models;
using OracleTools2026.Services;
using OracleTools2026.Settings;
using System.Net.NetworkInformation;

namespace OracleTools2026.UI;

public partial class DirectoryLoginForm : Form
{
    private const string DefaultPort = "389";

    private readonly ActiveDirectoryService _directory;
    private readonly ISavedDirectoryConnectionStore _savedConnections;

    public DirectoryLoginForm(ActiveDirectoryService directory, ISavedDirectoryConnectionStore savedConnections)
    {
        _directory = directory;
        _savedConnections = savedConnections;
        InitializeComponent();
    }

    public DirectoryConnection? Connection { get; private set; }

    public DirectoryLoginForm WithConnection(DirectoryConnection? connection)
    {
        Connection = connection;
        return this;
    }

    private void DirectoryLoginForm_Load(object? sender, EventArgs e)
    {
        if (Connection is { } current)
        {
            ApplyConnection(current);
        }
        else if (_savedConnections.GetAll() is [var last, ..])
        {
            ApplySavedConnection(last);
        }
        else
        {
            domainTextBox.Text = GetComputerDomain();
            portTextBox.Text = DefaultPort;
        }

        ActiveControl = domainTextBox.Text.Length == 0 ? domainTextBox : userTextBox.Text.Length == 0 ? userTextBox : passwordTextBox;
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
        var connection = new DirectoryConnection(domainTextBox.Text.Trim(), portTextBox.Text.Trim(), userTextBox.Text.Trim(), passwordTextBox.Text);
        if (!connection.UsesWindowsCredentials && connection.Password.Length == 0)
        {
            MessageBox.Show(this, "Informe a senha ou deixe o usuário em branco para usar a conta do Windows.", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        UseWaitCursor = true;
        mainLayout.Enabled = false;
        try
        {
            await Task.Run(() => _directory.Test(connection));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Erro ao conectar no Active Directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        Connection = connection;
        DialogResult = DialogResult.OK;
        Close();
    }

    private bool TrySaveConnection(DirectoryConnection connection)
    {
        var alias = _savedConnections.Find(connection)?.Alias
                    ?? InputDialog.Show(this, "Apelido da conexão", "Informe o apelido da conexão:", connection.ToString());

        if (string.IsNullOrWhiteSpace(alias))
        {
            return false;
        }

        var saved = savePasswordCheckBox.Checked ? connection : connection with { Password = "" };
        _savedConnections.Save(new SavedDirectoryConnection(alias, saved));
        return true;
    }

    private void ApplySavedConnection(SavedDirectoryConnection saved)
    {
        ApplyConnection(saved.Connection);
        saveConnectionCheckBox.Checked = true;
        savePasswordCheckBox.Checked = saved.HasPassword;
        ActiveControl = saved.HasPassword || saved.Connection.UsesWindowsCredentials ? okButton : passwordTextBox;
    }

    private void ApplyConnection(DirectoryConnection connection)
    {
        domainTextBox.Text = connection.Domain;
        portTextBox.Text = connection.Port;
        userTextBox.Text = connection.User;
        passwordTextBox.Text = connection.Password;
    }

    private void RemoveSavedConnection(SavedDirectoryConnection connection)
    {
        var answer = MessageBox.Show(this, $"Tem certeza que deseja apagar a conexão \"{connection.Alias}\"?",
            "Apagar conexão salva", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            _savedConnections.Remove(connection.Alias);
        }
    }

    private static string GetComputerDomain()
    {
        try
        {
            return IPGlobalProperties.GetIPGlobalProperties().DomainName;
        }
        catch (NetworkInformationException)
        {
            return "";
        }
    }
}
