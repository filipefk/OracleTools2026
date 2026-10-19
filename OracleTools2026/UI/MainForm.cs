using OracleTools2026.Data;
using OracleTools2026.Services;
using System.Reflection;

namespace OracleTools2026.UI;

public partial class MainForm : Form
{
    private readonly IOracleSession _session;
    private readonly ToolWindowManager _toolWindows;
    private readonly ToolLauncher _launcher;
    private readonly Func<LoginForm> _loginFormFactory;
    private readonly Func<SettingsForm> _settingsFormFactory;
    private readonly List<Label> _groupLabels = [];

    public MainForm(IOracleSession session, ToolWindowManager toolWindows, ToolLauncher launcher,
        Func<LoginForm> loginFormFactory, Func<SettingsForm> settingsFormFactory)
    {
        _session = session;
        _toolWindows = toolWindows;
        _launcher = launcher;
        _loginFormFactory = loginFormFactory;
        _settingsFormFactory = settingsFormFactory;

        InitializeComponent();
        AppIcon.ApplyTo(this);
        CreateToolTiles();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        versionLabel.Text = $"Versão {version?.ToString(3)}";

        UpdateConnectionLabel();
        _session.ConnectionChanged += Session_ConnectionChanged;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel || _toolWindows.OpenForms.Count == 0)
        {
            return;
        }

        var answer = MessageBox.Show(this, $"Há {_toolWindows.OpenForms.Count} ferramenta(s) aberta(s). Fechar todas e sair?",
            Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        e.Cancel = answer != DialogResult.Yes || !_toolWindows.CloseAll();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _session.ConnectionChanged -= Session_ConnectionChanged;
        base.OnFormClosed(e);
    }

    private void Session_ConnectionChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => Session_ConnectionChanged(sender, e));
            return;
        }

        UpdateConnectionLabel();
    }

    private void UpdateConnectionLabel() =>
        connectionLabel.Text = _session.Current is { IsComplete: true } current
            ? $"Conectado em {current.User}@{current.DataSource}"
            : "Não conectado";

    private void CreateToolTiles()
    {
        toolsPanel.SuspendLayout();
        foreach (var group in ToolCatalog.Tools.GroupBy(t => t.Group))
        {
            var groupLabel = new Label
            {
                Text = group.Key,
                AutoSize = false,
                Font = new Font(Font.FontFamily, Font.SizeInPoints * 1.15f, FontStyle.Bold),
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(6, _groupLabels.Count == 0 ? 0 : 12, 6, 0),
                TextAlign = ContentAlignment.BottomLeft
            };
            groupLabel.Height = groupLabel.Font.Height + 6;
            _groupLabels.Add(groupLabel);
            toolsPanel.Controls.Add(groupLabel);
            toolsPanel.SetFlowBreak(groupLabel, true);

            ToolTileButton? lastTile = null;
            foreach (var tool in group)
            {
                lastTile = new ToolTileButton(tool) { TabIndex = toolsPanel.Controls.Count };
                lastTile.Click += ToolTile_Click;
                toolTip.SetToolTip(lastTile, $"{tool.Description}\n\nLinha de comando: OracleTools2026.exe {tool.Command}");
                toolsPanel.Controls.Add(lastTile);
            }

            if (lastTile is not null)
            {
                toolsPanel.SetFlowBreak(lastTile, true);
            }
        }

        toolsPanel.ResumeLayout();
        ResizeGroupLabels();
    }

    private void ToolsPanel_ClientSizeChanged(object? sender, EventArgs e) => ResizeGroupLabels();

    private void ResizeGroupLabels()
    {
        var width = toolsPanel.ClientSize.Width - toolsPanel.Padding.Horizontal - 12;
        foreach (var label in _groupLabels)
        {
            label.Width = Math.Max(100, width);
        }
    }

    private void ToolTile_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolTileButton { Tool: var tool })
        {
            return;
        }

        try
        {
            _toolWindows.Open(tool);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Não foi possível abrir {tool.Title}:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConnectMenuItem_Click(object? sender, EventArgs e)
    {
        using var form = _loginFormFactory();
        form.ShowDialog(this);
    }

    private void NewInstanceMenuItem_Click(object? sender, EventArgs e)
    {
        try
        {
            _launcher.LaunchNewInstance();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Não foi possível abrir uma nova instância:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsMenuItem_Click(object? sender, EventArgs e)
    {
        using var form = _settingsFormFactory();
        form.ShowDialog(this);
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e) => Close();
}
