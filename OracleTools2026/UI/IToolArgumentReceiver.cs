namespace OracleTools2026.UI;

// Telas que aceitam um argumento extra ao abrir (ex.: o nome do usuário em DETALHES_USER pela linha de comando).
public interface IToolArgumentReceiver
{
    void SetToolArgument(string argument);
}
