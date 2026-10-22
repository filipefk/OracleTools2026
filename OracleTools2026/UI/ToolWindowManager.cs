using Microsoft.Extensions.DependencyInjection;
using OracleTools2026.Models;

namespace OracleTools2026.UI;

// Abre as ferramentas como janelas do mesmo processo, para que todas compartilhem a conexão do IOracleSession.
// Os formulários são criados com ActivatorUtilities para que o container não guarde referência a cada janela aberta.
public sealed class ToolWindowManager(IServiceProvider serviceProvider)
{
    private readonly List<Form> _openForms = [];

    public IReadOnlyList<Form> OpenForms => _openForms;

    public Form Create(ToolDefinition tool, string? argument = null)
    {
        var form = (Form)ActivatorUtilities.CreateInstance(serviceProvider, tool.FormType);
        if (!string.IsNullOrWhiteSpace(argument) && form is IToolArgumentReceiver receiver)
        {
            receiver.SetToolArgument(argument.Trim());
        }

        return form;
    }

    public void Open(ToolDefinition tool, string? argument = null)
    {
        var form = Create(tool, argument);
        form.FormClosed += (_, _) => _openForms.Remove(form);

        _openForms.Add(form);
        form.Show();
    }

    public void Open<TForm>(string? argument = null) where TForm : Form =>
        Open(ToolCatalog.Find(typeof(TForm)) ?? throw new InvalidOperationException($"{typeof(TForm).Name} não está no catálogo."), argument);

    // Retorna false se alguma janela cancelar o próprio fechamento.
    public bool CloseAll()
    {
        foreach (var form in _openForms.ToList())
        {
            form.Close();
        }

        return _openForms.Count == 0;
    }
}
