# Oracle Tools 2026

Conjunto de utilitários para administração e gestão de mudanças em bancos de dados Oracle.

É a reescrita completa do [OracleTools](../OracleTools) original (.NET Framework 2.0, x86, 2013), agora em **.NET 10**, **x64**, com driver Oracle 100% gerenciado e práticas atuais de desenvolvimento. As funcionalidades estão sendo migradas aos poucos.

## Tecnologias

| Item | Detalhe |
|------|---------|
| Linguagem | C# (nullable habilitado) |
| Interface | WinUI 3 / [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/) 2.6 (`net10.0-windows10.0.26100.0`), Fluent Design do Windows 11 (Mica, NavigationView, TitleBar, InfoBar, ContentDialog), temas claro e escuro |
| Padrão | MVVM com [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) |
| Listas | [WinUI.TableView](https://github.com/w-ahmad/WinUI.TableView) (ordenação, filtro e redimensionamento de colunas) |
| Plataforma | x64, app não empacotado (sem MSIX), com o Windows App SDK incluído na pasta do programa |
| Acesso ao Oracle | [Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core) (não exige Oracle Client instalado) |
| Injeção de dependência | `Microsoft.Extensions.Hosting` (Generic Host) |
| Active Directory | `System.DirectoryServices` (LDAP) |
| Preferências | Registro do Windows (`HKCU\Software\ORACLE_Tools\Parametros`) |

O app não é empacotado em MSIX de propósito: o MSIX virtualiza o `HKCU`, e as preferências deixariam de ser compartilhadas com o OracleTools original.

## Estrutura do projeto

```
OracleTools2026.slnx
Release.cmd                     # Compila em Release e gera o instalador
OracleTools2026.Core/           # Biblioteca sem interface
├── Models/                     # Records de domínio (DatabaseObject, DatabaseSession, SchedulerJob, DatabaseLink...)
├── Settings/                   # Preferências no registro, conexões salvas e criptografia compatível com o original
├── Data/                       # Acesso ao Oracle: sessão, listagens, DDL, usuários, recompilação, sessões, jobs, DBLINKs, scripts
├── Services/                   # Saída da DDL, arquivos auxiliares, nomes reais, Active Directory, nova instância, aviso sonoro
├── Formatting/                 # Formatos das listas (datas, durações, valores, texto separado por TAB)
├── Presentation/               # Interfaces usadas pelos ViewModels (IToolHost, IClipboardService, IToolNavigator...)
└── ViewModels/                 # Um ViewModel por ferramenta e diálogo (base ToolViewModelBase)
OracleTools2026/                # App WinUI 3
├── Instalador/Instalador.iss   # Script do Inno Setup
├── App.xaml(.cs)               # Host + DI, roteamento por argumento de linha de comando
├── ToolCatalog.cs              # Ferramentas do menu e da linha de comando
├── Views/                      # MainWindow (menu lateral), ToolWindow (ferramenta em janela própria), HomePage, SettingsPage
│   ├── Tools/                  # Uma página por ferramenta (base ToolPage)
│   └── Dialogs/                # Login no Oracle e no AD, informações do banco
├── Controls/                   # ToolStatusBar (status, progresso e Cancelar)
├── Styles/                     # Estilos e cores das listas
└── UI/                         # Janelas, diálogos, área de transferência, navegação entre ferramentas, utilitários das listas
```

## Funcionalidades

### Janela principal
- Aberta ao executar o programa sem parâmetros. Menu lateral com a página **Início** (as ferramentas em cartões, agrupadas em Objetos de banco, Usuários e Monitoramento), as ferramentas e, no rodapé, **Parâmetros**.
- As ferramentas abrem como páginas na janela principal e **mantêm o estado** ao trocar de uma para outra. Com o botão direito numa ferramenta do menu, **Abrir em nova janela** permite usar duas ferramentas lado a lado.
- Todas as ferramentas do processo (na janela principal ou em janelas próprias) **compartilham a conexão**: conectar (botão de conexão na barra de título, Ctrl+L, ou por qualquer ferramenta) troca a conexão de todas. Ao trocar, cada ferramenta descarta os dados do banco anterior e recarrega as listas (se houver uma operação em andamento, ao final dela). A barra de título mostra a conexão atual.
- Para usar outro banco ao mesmo tempo, **Nova instância** (Ctrl+N ou botão na barra de título) abre outro processo do programa, com a sua própria conexão. Ferramentas chamadas pela linha de comando também rodam em processo isolado.
- Fechar a janela principal fecha também as ferramentas abertas em outras janelas (com confirmação).
- Cada ferramenta tem no topo o banco conectado e os comandos, uma barra de avisos (InfoBar) para mensagens e erros, e no rodapé o status, o progresso e o botão **Cancelar** (Esc) da operação em andamento.
- Nas listas: ordenação, filtro e redimensionamento pelo cabeçalho das colunas; Ctrl+A seleciona tudo; Ctrl+C copia as linhas selecionadas (com o cabeçalho, separadas por TAB); o botão direito abre o menu da linha, que muda conforme a linha.
- A lista de ferramentas fica em `ToolCatalog.cs`, usada também pelo roteamento da linha de comando.

### Extrair DDL de objetos de banco (`ExtractDdlPage`)
- Lista objetos filtrando por owner, tipo e parte do nome.
- Lista as **últimas compilações** (objetos alterados nos últimos N dias) e os **objetos inválidos**, com as mensagens de erro de compilação.
- **Recompila objetos inválidos** em tentativas sucessivas (types, packages, procedures, functions, triggers, views e sinônimos) e gera um relatório do que foi compilado.
- Extrai a DDL de procedures, functions, packages (spec + body), triggers (inclusive `DISABLE`), types (spec + body), views, sequences, jobs do `DBMS_SCHEDULER`, Java sources, sinônimos e grants.
- Extrai dos **objetos marcados**, de **toda a lista** ou de uma **lista digitada** (`OWNER.NOME`, um por linha).
- Destino: **arquivos** numa pasta (`Owner\Tipo\OWNER.NOME.ext`, em UTF-8), **área de transferência** ou **editor de texto** padrão do Windows.
- Opções de incluir um **cabeçalho** (dados da conexão, datas e status do objeto) e as **mensagens de erro de compilação** nas linhas correspondentes.
- Ordenação por coluna, cópia das linhas da lista (Ctrl+C), menu de contexto e cancelamento das operações em andamento.

Extensões dos arquivos gerados:

| Tipo | Extensão |
|------|----------|
| PROCEDURE | `.prc` |
| FUNCTION | `.fnc` |
| PACKAGE | `.pkb` |
| TRIGGER | `.trg` |
| VIEW | `.vw` |
| TYPE | `.typ` |
| JAVA SOURCE | `.jsp` |
| SEQUENCE, JOB | `.sql` |

### Extrair script de criação de usuários (`ExtractUserPage`)
- Lista os usuários do banco filtrando por owner (todos, que sejam owner de objetos ou que não sejam), profile, parte do nome e um **filtro SQL** livre opcional (condição extra, sem `WHERE` nem o primeiro `AND`, guardada na preferência `FiltroPesquisaUsuarios` do original).
- Colunas: nome, criação, profile, status, data de lock, data de expiração e último login. O último login vem de `DBA_USERS.LAST_LOGIN` (Oracle 12c+; em versões anteriores fica em branco). Usuários bloqueados aparecem em cinza.
- Gera o script DDL de criação do usuário: `CREATE USER` (senha `<SENHA_USUARIO>` a definir), tablespaces, profile, cotas, `ACCOUNT LOCK`/`PASSWORD EXPIRE` conforme o status, privilégios de sistema, roles (com as roles default), grants de objetos e de colunas concedidos diretamente ao usuário.
- Extrai dos **usuários marcados**, de **toda a lista** ou de uma **lista digitada** (um por linha), para **arquivos** (`<pasta>\USUARIOS\NOME.sql`, em UTF-8), **área de transferência** ou **editor de texto**. Duplo clique abre o script do usuário no editor.
- Menu de contexto: ver/copiar o script, abrir os detalhes (permissões) do usuário e copiar linhas.
- Correções em relação ao original: grants de objetos usam `WITH GRANT OPTION` (o original gerava `WITH ADMIN OPTION`, inválido), grants de diretório saem sem owner (`ON DIRECTORY NOME`) e o tipo do objeto vem do dicionário (o original tratava todo `READ`/`WRITE` como diretório).

### Detalhes de um usuário (`UserDetailsPage`)
- **Permissões no banco**: árvore com status, tablespaces, profile (limites `KERNEL`/`PASSWORD`, mostrando o valor efetivo dos limites `DEFAULT`), datas de criação, lock, expiração e último login, cotas, privilégios de sistema, roles e grants diretos agrupados por tipo de objeto.
- Roles mostram seus privilégios de sistema, roles concedidas a elas e grants por tipo de objeto. Os nós são carregados sob demanda; **Expandir tudo** carrega e expande a árvore inteira.
- **Gerar relatório** abre no editor os dados do banco e os nós expandidos, indentados; **Extrair script do usuário** gera o script de criação. Botão direito em um grant de procedure, function, package, view, sequence, trigger ou type abre a DDL do objeto.
- **Localizar usuário**: procura por nome ou login (aceita `*` e `%`) no **Active Directory** (nome de exibição, login, descrição, e-mail, departamento, cargo, conta desabilitada/bloqueada, senha nunca expira, trocar senha no próximo logon, datas de último logon e troca de senha, grupos) e no **banco** (`DBA_USERS`).
- A conexão com o AD pede domínio, porta, usuário e senha; com usuário em branco usa a conta do Windows. As conexões podem ser salvas (chave `ConexoesAD`, no mesmo formato encriptado do original).
- A busca no SVN do original não foi migrada (o projeto não terá vínculo com SVN).

### Procurar lock de banco (`LockMonitorPage`)
- Executa periodicamente `ScriptLock01.sql` (botão Script 1) ou `ScriptLock02.sql` (Script 2) e mostra o resultado; o script é relido a cada início.
- Intervalo configurável na tela (padrão de 5 segundos, como no original).
- Com **Avisar caso encontre lock** marcado, traz a janela para frente e emite um aviso sonoro quando o script retorna linhas.
- Um erro na verificação aparece na barra de avisos, sem interromper o monitoramento (no original, a mensagem modal parava as verificações até ser fechada).
- Aceita scripts no formato do SQL*Plus (`;` ou `/` no final) e arquivos em ANSI ou UTF-8.
- Botão direito nos botões de script copia o script.

### Sessões ativas (`SessionsPage`)
- Lista as sessões de `GV$SESSION` (inclusive a instância, em RAC) com usuário, usuário do SO, máquina, programa, módulo, logon e estado.
- Preenche automaticamente o **nome real** do usuário do banco e do usuário do SO a partir de `Lista_Usuarios2.txt`, guardando o que encontrar em `ListaNomesAd.txt` (mesmos arquivos do original).
- Destaca com fundo cinza as sessões em que o usuário do banco e o do SO são de pessoas diferentes.
- **Mata** as sessões marcadas e/ou selecionadas (botão, menu de contexto ou tecla Delete), com confirmação e relatório dos erros. Na instância da conexão usa `ALTER SYSTEM DISCONNECT SESSION ... IMMEDIATE`; em outra instância do RAC, `KILL SESSION 'sid,serial#,@inst_id'`.

### Jobs de banco (`SchedulerJobsPage`)
- Lista os jobs de `ALL_SCHEDULER_JOBS` com criação, última alteração, ação, status, tipo, programa, agenda, intervalo, última/próxima execução, duração, quantidade de execuções e falhas, comentários e estado.
- Coluna **Situação** e cores: desabilitado (cinza), atrasado/possivelmente travado (vermelho) e rodando há mais de 1 hora (laranja). As comparações de datas são feitas no banco, respeitando o fuso horário.
- Duplo clique abre a ação do job no editor de texto; menu de contexto para copiar a ação ou as linhas.

### DBLINKs (`DatabaseLinksPage`)
- Lista os database links de `ALL_DB_LINKS` e testa a conectividade de **todos** ou dos **selecionados**, mostrando "TESTE OK" (verde) ou a mensagem de erro (vermelho) em cada linha conforme os testes terminam.
- Cada link é fechado após o teste, evitando o erro de limite de links abertos (ORA-02020) ao testar muitos de uma vez.

### Arquivos auxiliares
Os arquivos `ScriptLock01.sql`, `ScriptLock02.sql`, `Lista_Usuarios2.txt` e `ListaNomesAd.txt` são procurados primeiro em `%APPDATA%\OracleTools2026` e depois na pasta do executável. Como a pasta de instalação não tem permissão de escrita, o `ListaNomesAd.txt` é sempre gravado em `%APPDATA%\OracleTools2026`.

### Ainda não migradas
Detalhes de role, controle de concorrência, backups de objetos, alerta baseado em consulta, agendamento de scripts.

## Execução por linha de comando

```
OracleTools2026.exe <FERRAMENTA> [usuario senha alias_tns] [nome]
```

| Argumento | Ferramenta |
|-----------|------------|
| `EXTRACT_DDL_SOURCE` | Extrair DDL de objetos |
| `EXTRACT_DDL_USER` | Extrair script de usuários |
| `DETALHES_USER` | Detalhes de um usuário (aceita o nome do usuário como 5º argumento) |
| `PROCURA_LOCK_BANCO` | Procurar lock |
| `SESSOES` | Sessões ativas |
| `JOBS_DE_BANCO` | Jobs de banco |
| `DBLINKS` | DBLINKs |

Sem argumento ou com um argumento desconhecido, abre a janela principal. Com uma ferramenta, ela abre sozinha numa janela própria. Com usuário, senha e alias, a ferramenta já abre conectada (Sessões, Jobs e DBLINKs já listam ao abrir; Detalhes de um usuário já pesquisa o usuário informado).

## Conexão e preferências

- A conexão usa os aliases do **`tnsnames.ora`**, cujo caminho o usuário seleciona na primeira vez (a pasta do arquivo é usada como `TNS_ADMIN` do driver).
- As preferências ficam no registro em `HKEY_CURRENT_USER\Software\ORACLE_Tools\Parametros`, **as mesmas chaves do OracleTools original**: caminho do tnsnames, último login, pasta padrão de extração, opções de cabeçalho e de mensagens de erro, filtro SQL de usuários, conexões salvas do Oracle e do Active Directory (encriptadas, no mesmo formato do original).
- As telas de usuários consultam as views `DBA_*` (`DBA_USERS`, `DBA_TS_QUOTAS`, `DBA_SYS_PRIVS`, `DBA_ROLE_PRIVS`, `DBA_TAB_PRIVS`, `DBA_COL_PRIVS`, `DBA_PROFILES`), o que exige `SELECT_CATALOG_ROLE` ou privilégio equivalente.
- Os parâmetros podem ser editados em **Parâmetros**, no rodapé do menu lateral da janela principal (as conexões salvas, encriptadas, aparecem só para leitura).

## Requisitos

- Windows 10 (versão 2004 ou superior) ou Windows 11, x64.
- .NET 10 Runtime x64 (o .NET 10 Desktop Runtime também serve, pois o inclui). O Windows App SDK vai junto com o programa.
- Acesso de rede ao banco Oracle e um `tnsnames.ora` com os aliases.

## Compilação

Abra `OracleTools2026.slnx` no Visual Studio 2026 (ou superior), com a carga de trabalho de desenvolvimento de aplicativos WinUI, e compile na plataforma **x64**. Os pacotes NuGet são restaurados automaticamente.

```bash
dotnet build OracleTools2026.slnx -c Release -p:Platform=x64
```

## Instalador

Instalador para Windows x64 gerado com [Inno Setup](https://jrsoftware.org/isinfo.php). Instala em `C:\Program Files\OracleTools2026`, cria atalho no menu Iniciar (e, opcionalmente, na área de trabalho). A desinstalação remove tudo isso; as preferências no registro (`HKCU\Software\ORACLE_Tools`) são mantidas, pois são compartilhadas com o OracleTools original.

O computador de destino precisa ter o **.NET 10 Runtime** x64 (ou o Desktop Runtime) instalado. O Windows App SDK é copiado junto com o programa (instalação self-contained do Windows App SDK), sem instalador separado.

Para gerar (requer o `iscc` do Inno Setup no PATH):

1. Atualize a versão em `OracleTools2026\Instalador\Instalador.iss` (`AppVersion` e `OutputBaseFilename`) em `OracleTools2026\OracleTools2026.csproj` (`AssemblyVersion`, `FileVersion` e `InformationalVersion`) e em `OracleTools2026\app.manifest` (`assemblyIdentity version`, no formato `X.Y.Z.0`).
2. Execute `Release.cmd` na raiz: compila a solução em Release, apaga o instalador anterior e gera `OracleTools2026\Instalador\OracleTools2026-X.Y.Z.exe`.
