namespace OracleTools2026.Settings;

public static class SettingNames
{
    public const string TnsNamesPath = "CaminhoTnsNames";
    public const string LastLoginUser = "UltimoLoginUsuario";
    public const string LastLoginDataSource = "UltimoLoginDatabase";
    public const string DefaultOutputFolder = "PastaPadraoSalvarArquivos";
    public const string IncludeHeaderOnDdlExtraction = "ColocarCabecalhoAoExtrairDDL";
    public const string InlineCompileErrorsOnDdlExtraction = "ColocarMensagemErroNaLinhaAoExtrairDDL";
    public const string SavedOracleConnections = "ConexoesORACLE";

    private static readonly Dictionary<string, string> FriendlyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        [TnsNamesPath] = "Caminho do tnsnames.ora",
        [LastLoginUser] = "Último usuário logado",
        [LastLoginDataSource] = "Último banco logado",
        [DefaultOutputFolder] = "Pasta padrão para salvar arquivos ao extrair DDL",
        [IncludeHeaderOnDdlExtraction] = "Colocar cabeçalho ao extrair DDL",
        [InlineCompileErrorsOnDdlExtraction] = "Colocar mensagem de erro na linha ao extrair DDL",
        [SavedOracleConnections] = "Conexões de banco ORACLE salvas encriptadas",
        ["ConexoesFTPSalvas"] = "Conexões FTP salvas encriptadas",
        ["ConexoesSVNSalvas"] = "Conexões SVN salvas encriptadas",
        ["CaminhoExeTortoiseSVN"] = "Caminho do executável do Tortoise SVN",
        ["FiltroPesquisaUsuarios"] = "Filtro padrão para pesquisa de usuário sem filtro",
        ["PastaImagensRelatoriosHtml"] = "Pasta de imagens para relatórios HTML",
        ["CaminhoListagemTrunk"] = "Caminho do arquivo de listagem do Trunk",
        ["CaminhoListagemBranches"] = "Caminho do arquivo de listagem dos branches"
    };

    public static string GetFriendlyName(string name) =>
        FriendlyNames.TryGetValue(name, out var friendlyName) ? friendlyName : name;
}
