# CLAUDE.md

Orientações para trabalhar neste repositório. Visão geral do produto e das funcionalidades no [README.md](README.md).

## Contexto

- Reescrita do zero do OracleTools original (`D:\Fontes\OracleTools`, .NET Framework 2.0, x86, ODP.NET 11.2 + OraOLEDB) para **.NET 10, WinUI 3 (Windows App SDK), x64**, com o visual Fluent do Windows 11. A primeira versão foi feita em WinForms e depois convertida para WinUI 3, com as telas reprojetadas e as mesmas funcionalidades.
- A migração é feita **por ferramenta**. Antes de migrar uma tela, leia o `frm*.cs` correspondente e os métodos usados em `csOracle.cs` / `csUtil.cs` do projeto original — eles são a referência de **regras de negócio e SQL**, nunca de estrutura ou estilo de código.
- O objetivo é manter as mesmas funcionalidades usando ferramentas e práticas atuais; pode-se melhorar comportamento (ex.: suporte a TYPE, bugs do original), mas sem remover funcionalidade.
- **Não pode haver nenhum vínculo com SVN** (nem caminhos como `D:\SVN_AES`, nem SharpSvn/TortoiseSVN). Uma integração com git será feita no futuro.

## Arquitetura

```
OracleTools2026.Core/      # biblioteca sem interface (não referencia WinUI)
├── Models/                # records imutáveis (sem lógica de banco)
├── Settings/              # IAppSettings (registro), conexões salvas (Oracle e AD, via ObfuscatedRecordList), LegacyCipher, SettingNames
├── Data/                  # IOracleSession + repositórios/serviços de banco (DatabaseObjectRepository, DdlExtractor, DatabaseUserRepository, UserScriptGenerator, ScriptRunner...)
├── Services/              # saída/integração com o SO (DdlOutputService, AppFiles, RealNameDirectory, ActiveDirectoryService, ToolLauncher = nova instância, SystemSound)
├── Formatting/            # DisplayFormat (datas, durações, valores, bytes) e TabularText (TSV com cabeçalho)
├── Presentation/          # interfaces de interface usadas pelos ViewModels: IToolHost, IClipboardService, IToolNavigator, IToolArgumentReceiver, ToolCommands
└── ViewModels/            # ToolViewModelBase + um ViewModel por ferramenta/diálogo, itens de linha (CheckableItem, ItemList, CheckableItemList)
OracleTools2026/           # app WinUI 3 (não empacotado)
├── App.xaml(.cs)          # Generic Host + DI; roteia a ferramenta pelo 1º argumento (ex.: EXTRACT_DDL_SOURCE)
├── ToolCatalog.cs         # ferramentas: comando, título, descrição, grupo, glifo e tipo da página
├── Views/                 # MainWindow (NavigationView), ToolWindow (uma ferramenta), HomePage, SettingsPage, ToolPage (base)
│   ├── Tools/             # *Page.xaml de cada ferramenta
│   └── Dialogs/           # ContentDialogs: LoginDialog, DirectoryLoginDialog, DatabaseInfoDialog
├── Controls/              # ToolStatusBar
├── Styles/AppStyles.xaml  # estilos (TableView, cartões, cores de linha)
└── UI/                    # WindowService, ToolHost (IToolHost), DialogService (fila de ContentDialog por janela), ClipboardService, ToolNavigator, TableViewHelper, MenuBuilder, BindingHelpers, Glyphs
```

- **MVVM**: a lógica das telas fica nos ViewModels (`CommunityToolkit.Mvvm`: `[ObservableProperty]` em propriedades `partial`, `[RelayCommand]`). O code-behind das páginas só faz o que é de interface (menus de contexto montados na hora, colunas dinâmicas, foco, atalhos). ViewModels não referenciam tipos do WinUI; o que depende da janela passa por `IToolHost` (diálogos, seletor de pasta, trazer à frente) e `IClipboardService`.
- **Menu e ferramentas**: `ToolCatalog.cs` lista as ferramentas. O `App` roteia pela linha de comando; a `MainWindow` monta o menu lateral a partir do catálogo e abre cada ferramenta como página (em cache: `NavigationCacheMode.Required`, mantém o estado). "Abrir em nova janela" e a linha de comando usam `ToolWindow`. Todas as janelas do processo compartilham a conexão do `IOracleSession`; "Nova instância" (`ToolLauncher`) abre outro processo. Ao criar uma ferramenta: ViewModel em `Core/ViewModels/<Ferramenta>/`, página em `Views/Tools/`, registro do ViewModel em `App.CreateHost`, comando em `ToolCommands` e entrada no catálogo.
- **Argumento de abertura**: ViewModels que aceitam o 5º argumento da linha de comando (ex.: nome do usuário em `DETALHES_USER`) implementam `IToolArgumentReceiver`; a `ToolPage` repassa o parâmetro da navegação. Para abrir uma ferramenta a partir de outra, use `IToolNavigator.Open(ToolCommands.X, argumento)`.
- **Páginas de ferramenta** herdam de `ToolPage` (XAML com raiz `views:ToolPage`), obtêm o ViewModel com `App.GetService<T>()` antes de `InitializeComponent` e sobrescrevem `ToolViewModel`. A `ToolPage` associa o `ToolHost` da janela, chama `ActivateAsync` na primeira exibição e `Deactivate` ao fechar a janela. Layout padrão: cabeçalho (título + `ServerInfo`) com `CommandBar`, `InfoBar` ligada a `IsNoticeOpen`/`NoticeKind`/`NoticeMessage`, conteúdo e `ToolStatusBar` no rodapé.
- **ViewModels de ferramenta** herdam de `ToolViewModelBase` (recebe `ToolServices`): `Title`/`ServerInfo`, `IsBusy`/`IsIdle`, `StatusText`, progresso, `ConnectCommand`, `ShowServerInfoCommand`, `CancelCommand`, `RunAsync` (com `requiresConnection: false` para operações que não usam o banco), `EnsureConnectedAsync`, `AskAsync`/`ShowMessageAsync` (diálogos) e `ShowInfo`/`ShowSuccess`/`ShowWarning`/`ShowError` (InfoBar, não bloqueiam). Comandos usam `CanExecute = nameof(IsIdle)`; a base avisa todos os comandos quando `IsBusy` muda. Controles que não são comandos usam `IsEnabled="{x:Bind ViewModel.IsIdle, Mode=OneWay}"`.
- **Troca de conexão**: qualquer tela pode trocar a conexão de todas. ViewModels que listam dados do banco sobrescrevem `OnConnectionChangedAsync` para descartar/recarregar a lista (a base adia a chamada até o fim de uma operação em andamento e a executa na thread de UI).
- **Diálogos**: só pode haver um `ContentDialog` aberto por janela; use sempre `UI/DialogService` (fila por janela; a classe não se chama `Dialogs` para não colidir com o namespace `Views.Dialogs`). Dentro de um diálogo, não abra outro: confirmações usam `Flyout` e erros aparecem numa `InfoBar` do próprio diálogo.
- **Listas**: `WinUI.TableView` com o estilo `ToolTableViewStyle`, colunas `TableViewTextColumn` ligadas a propriedades de texto já formatadas pelo item (`DisplayFormat`) e `SortMemberPath` apontando para o valor original (datas, durações) para ordenar corretamente. Linhas com checkbox: itens derivados de `CheckableItem` em `CheckableItemList` (contagem e "Selecionar todos"). Cores de linha via `ConditionalCellStyles` com predicados na página e os estilos `*CellStyle` de `AppStyles.xaml` (funcionam nos temas claro e escuro). `TableViewHelper.Attach` sincroniza a seleção com `ItemList.Selected` e faz o Ctrl+C copiar com cabeçalho; menus de contexto são montados em `RowContextFlyoutOpening` com `MenuBuilder`.
- **Arquivos auxiliares** (`AppFiles`): procurados em `%APPDATA%\OracleTools2026` e depois na pasta do executável; gravações vão para o `%APPDATA%`. Leitura aceita ANSI (formato do original) e UTF-8.

- `IOracleSession` (singleton) guarda a `ConnectionInfo` atual, abre conexões (pool do driver, uma conexão por operação) e expõe o evento `ConnectionChanged`.
- Consultas usam apenas objetos do próprio Oracle (dicionário de dados, `V$`/`GV$`, `DBMS_*`). **Não** referenciar tabelas, funções ou schemas de aplicação (ex.: `AESPROD`, `HORACIUS`, `ORADBA`), mesmo que o original os use.
- Classes de `Data/` e `Services/` **não acessam a UI**: lançam exceções ou retornam resultados (ex.: `DdlExtractionResult` com `Error`). Só os ViewModels decidem as mensagens, e só o app (WinUI) as mostra.
- Serviços e ViewModels são resolvidos pelo container (`App.CreateHost`); páginas e janelas não são registradas (o `Frame` cria as páginas). Ao criar um novo serviço/ViewModel, registre-o em `CreateHost`.
- Operações de banco nas telas: usar o `RunAsync` do ViewModel (desabilita comandos, mostra progresso, permite cancelar) e chamar os serviços dentro de `Task.Run(...)` para não bloquear a thread de UI.
- Progresso via `IProgress<ProgressInfo>`; cancelamento via `CancellationToken` em todos os métodos assíncronos.

## Convenções de código

- Nomenclatura Microsoft: PascalCase para tipos/membros, `_camelCase` para campos privados, `camelCase` para parâmetros/locais, `PascalCase` nos `x:Name` do XAML (`UsersTable`, `OwnerBox`), sufixos `Page`, `Dialog`, `Window` e `ViewModel`, sufixo `Async` em métodos assíncronos, `I` em interfaces. Identificadores em **inglês**; textos da interface e mensagens em **português**.
- C# moderno: file-scoped namespaces, `record`, primary constructors, collection expressions, pattern matching, raw string literals (`"""`) para SQL, `await using`.
- Um tipo por arquivo; `using` ordenados alfabeticamente (o projeto não coloca `System` primeiro).
- **Nunca** adicionar comentários XML `<summary>`. Comentários só quando explicam algo não óbvio (ex.: compatibilidade com o original).
- SQL sempre com **bind variables** (`connection.CreateTextCommand(sql, ("nome", valor))` em `OracleExtensions`). Concatenar no SQL apenas constantes do código. Identificadores em comandos DDL dinâmicos devem usar `OracleExtensions.Quote`.
- XAML: `{x:Bind}` (compilado) em vez de `{Binding}`, exceto nas colunas do `TableView` (que exigem `Binding`). `TextBox` ligado ao ViewModel usa `UpdateSourceTrigger=PropertyChanged` quando o valor é lido por atalho (ex.: Enter para pesquisar). Cores sempre por `ThemeResource` (nada fixo, para funcionar nos temas claro e escuro); glifos da Segoe Fluent Icons (`&#xE721;` no XAML, constantes em `UI/Glyphs` no C#).
- Textos e ícones seguem o Fluent Design: `CommandBar` com rótulo à direita, `InfoBar` para avisos, `ContentDialog` para confirmações, cartões (`CardPanelStyle`) para filtros e opções.

## Compatibilidade obrigatória com o original

- Preferências no registro em `HKCU\Software\ORACLE_Tools\Parametros`, **com os mesmos nomes de chave** (constantes em `Settings/SettingNames.cs`). Flags booleanas aceitam `1/0/TRUE/FALSE/SIM/NÃO` e são gravadas como `1`/`0`.
- Conexões salvas (`ConexoesORACLE` e `ConexoesAD`): manter o formato e a ofuscação de `ObfuscatedRecordList` e a criptografia de `LegacyCipher` (AES-CBC, PBKDF2-SHA1 1000 iterações, mesmas chaves), para que as duas versões leiam os mesmos dados.
- Linha de comando: `<FERRAMENTA> [usuario senha alias_tns]`, com os mesmos nomes de ferramenta do original (`EXTRACT_DDL_SOURCE`, `EXTRACT_DDL_USER`, `DETALHES_USER`, `SESSOES`, `DBLINKS`, `JOBS_DE_BANCO`...).

## Build

- Não compilar nem rodar build automaticamente após edições; o usuário compila no Visual Studio e informa os erros.
- Projetos: `OracleTools2026.Core/OracleTools2026.Core.csproj` (biblioteca) e `OracleTools2026/OracleTools2026.csproj` (app WinUI), ambos `net10.0-windows10.0.26100.0`, somente x64, `LangVersion` preview (propriedades `partial` do MVVM Toolkit). O `OracleTools2026.slnx` só tem a plataforma x64 e mapeia cada projeto explicitamente (`<Platform Project="x64" />`); sem esse mapeamento o Visual Studio avisa "unknown project configuration mappings". Ao adicionar um projeto à solução, inclua o mesmo mapeamento.
- O build deve ficar sem avisos (Debug e Release).
- App não empacotado (`WindowsPackageType=None`) e com o Windows App SDK self-contained (`WindowsAppSDKSelfContained=true`). **Não converter para MSIX**: o MSIX virtualiza o `HKCU` e quebra o compartilhamento das preferências com o original.
- Pacotes: Core — `Oracle.ManagedDataAccess.Core`, `System.DirectoryServices`, `CommunityToolkit.Mvvm`, `System.Security.Cryptography.Pkcs` (referência direta só para substituir a versão 8.0.1, vulnerável, trazida pelo driver Oracle); app — `Microsoft.WindowsAppSDK`, `Microsoft.Windows.SDK.BuildTools`, `Microsoft.Extensions.Hosting`, `CommunityToolkit.Mvvm`, `CommunityToolkit.WinUI.Controls.SettingsControls`, `WinUI.TableView`.

## Instalador

- Inno Setup (`iscc` no PATH), no mesmo modelo do projeto `D:\Fontes\MarkdownEditor`: `OracleTools2026\Instalador\Instalador.iss` e `Release.cmd` (raiz).
- Build **framework-dependent do .NET** (requer o .NET 10 Runtime x64) com o Windows App SDK incluído, instalação somente x64 em `{commonpf}\OracleTools2026`, copiando tudo de `OracleTools2026\bin\Release\net10.0-windows10.0.26100.0\win-x64\` (a plataforma não entra no caminho: `AppendPlatformToOutputPath=false`). Não grava nada no registro (as preferências são do programa e compartilhadas com o original).
- Antes de gerar, atualizar a versão em `Instalador.iss` (`AppVersion` e `OutputBaseFilename` = `OracleTools2026-X.Y.Z`) no `.csproj` do app (`AssemblyVersion`, `FileVersion`, `InformationalVersion`) e no `app.manifest` (`assemblyIdentity version` = `X.Y.Z.0`).
- `Release.cmd`: `dotnet build OracleTools2026.slnx -c Release -p:Platform=x64` → apaga o `OracleTools2026-*.exe` anterior → `iscc`. O `.exe` sai em `OracleTools2026\Instalador\` (ignorado no git).
