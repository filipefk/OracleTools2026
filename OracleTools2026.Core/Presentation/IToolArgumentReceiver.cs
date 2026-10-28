namespace OracleTools2026.Presentation;

// Ferramentas que aceitam um argumento extra ao abrir (ex.: o nome do usuário em DETALHES_USER pela linha de comando).
public interface IToolArgumentReceiver
{
    void SetToolArgument(string argument);
}
