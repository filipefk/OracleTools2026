namespace OracleTools2026.Presentation;

// Abre uma ferramenta do catálogo (pelo comando da linha de comando) a partir de outra.
public interface IToolNavigator
{
    void Open(string command, string? argument = null);
}
