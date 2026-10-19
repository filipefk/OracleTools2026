# CLAUDE.md

Orientações para trabalhar neste repositório. Visão geral do produto e das funcionalidades no [README.md](README.md).

## Contexto

- Reescrita do zero do OracleTools original (`D:\Fontes\OracleTools`, .NET Framework 2.0, x86, ODP.NET 11.2 + OraOLEDB) para **.NET 10, WinForms, x64**.
- A migração é feita **por ferramenta**. Antes de migrar uma tela, leia o `frm*.cs` correspondente e os métodos usados em `csOracle.cs` / `csUtil.cs` do projeto original — eles são a referência de **regras de negócio e SQL**, nunca de estrutura ou estilo de código.
- O objetivo é manter as mesmas funcionalidades usando ferramentas e práticas atuais; pode-se melhorar comportamento (ex.: suporte a TYPE, bugs do original), mas sem remover funcionalidade.
- **Não pode haver nenhum vínculo com SVN** (nem caminhos como `D:\SVN_AES`, nem SharpSvn/TortoiseSVN). Uma integração com git será feita no futuro.

## Arquitetura

```
OracleTools2026/
├── Program.cs     # Generic Host + DI; roteia a ferramenta pelo 1º argumento (ex.: EXTRACT_DDL_SOURCE)
├── Models/        # records imutáveis (sem lógica de banco)
├── Settings/      # IAppSettings (registro), ISavedConnectionStore, LegacyCipher, SettingNames
├── Data/          # IOracleSession + repositórios/serviços de banco (DatabaseObjectRepository, DdlExtractor, InvalidObjectRecompiler)
├── Services/      # saída/integração com o SO (DdlOutputService)
└── UI/            # *Form + *.Designer.cs, utilitários de tela
```

- `IOracleSession` (singleton) guarda a `ConnectionInfo` atual, abre conexões (pool do driver, uma conexão por operação) e expõe o evento `ConnectionChanged`. Também detecta e cacheia se existem as tabelas de auditoria `AESPROD.AUDITORIA_OBJETOS`.
- Classes de `Data/` **não acessam a UI**: lançam exceções ou retornam resultados (ex.: `DdlExtractionResult` com `Error`). Só os formulários mostram `MessageBox`.
- Formulários são resolvidos pelo container. Formulários abertos a partir de outro são injetados como `Func<TForm>` (registrado em `Program.CreateHost`). Ao criar uma nova tela/serviço, registre-o em `CreateHost`.
- Operações de banco na UI: usar o helper `RunAsync` da tela (desabilita controles, mostra progresso, permite cancelar) e chamar os serviços dentro de `Task.Run(...)` para não bloquear a thread de UI.
- Progresso via `IProgress<ProgressInfo>`; cancelamento via `CancellationToken` em todos os métodos assíncronos.

## Convenções de código

- Nomenclatura Microsoft: PascalCase para tipos/membros, `_camelCase` para campos privados, `camelCase` para parâmetros/locais e controles do designer (`ownerComboBox`, `extractButton`), sufixo `Form` nos formulários, sufixo `Async` em métodos assíncronos, `I` em interfaces. Identificadores em **inglês**; textos da interface e mensagens em **português**.
- C# moderno: file-scoped namespaces, `record`, primary constructors, collection expressions, pattern matching, raw string literals (`"""`) para SQL, `await using`.
- Um tipo por arquivo; `using` ordenados alfabeticamente (o projeto não coloca `System` primeiro).
- **Nunca** adicionar comentários XML `<summary>`. Comentários só quando explicam algo não óbvio (ex.: compatibilidade com o original).
- SQL sempre com **bind variables** (`connection.CreateTextCommand(sql, ("nome", valor))` em `OracleExtensions`). Concatenar no SQL apenas constantes do código. Identificadores em comandos DDL dinâmicos devem usar `OracleExtensions.Quote`.
- Arquivos `*.Designer.cs` devem continuar editáveis pelo designer do Visual Studio: só instruções simples em `InitializeComponent`, sem lógica. Eles são tratados como código gerado (sem contexto nullable), por isso não use anotações `?` neles.

## Compatibilidade obrigatória com o original

- Preferências no registro em `HKCU\Software\ORACLE_Tools\Parametros`, **com os mesmos nomes de chave** (constantes em `Settings/SettingNames.cs`). Flags booleanas aceitam `1/0/TRUE/FALSE/SIM/NÃO` e são gravadas como `1`/`0`.
- Conexões salvas (`ConexoesORACLE`): manter o formato e a ofuscação de `SavedConnectionStore` e a criptografia de `LegacyCipher` (AES-CBC, PBKDF2-SHA1 1000 iterações, mesmas chaves), para que as duas versões leiam os mesmos dados.
- Linha de comando: `<FERRAMENTA> [usuario senha alias_tns]`, com os mesmos nomes de ferramenta do original (`EXTRACT_DDL_SOURCE`, `EXTRACT_DDL_USER`, `DETALHES_USER`, `SESSOES`, `DBLINKS`, `JOBS_DE_BANCO`...).

## Build

- Não compilar nem rodar build automaticamente após edições; o usuário compila no Visual Studio e informa os erros.
- Projeto: `OracleTools2026/OracleTools2026.csproj` (`PlatformTarget` x64). Pacotes: `Oracle.ManagedDataAccess.Core`, `Microsoft.Extensions.Hosting`.

## Instalador

- Inno Setup (`iscc` no PATH), no mesmo modelo do projeto `D:\Fontes\MarkdownEditor`: `OracleTools2026\Instalador\Instalador.iss` e `Release.cmd` (raiz).
- Build **framework-dependent** (requer .NET 10 Desktop Runtime), instalação somente x64 em `{commonpf}\OracleTools2026`, copiando tudo de `OracleTools2026\bin\Release\net10.0-windows\`. Não grava nada no registro (as preferências são do programa e compartilhadas com o original).
- Antes de gerar, atualizar a versão em `Instalador.iss` (`AppVersion` e `OutputBaseFilename` = `OracleTools2026-X.Y.Z`) e no `.csproj` (`AssemblyVersion`, `FileVersion`, `InformationalVersion`).
- `Release.cmd`: `dotnet build OracleTools2026.slnx -c Release` → apaga o `OracleTools2026-*.exe` anterior → `iscc`. O `.exe` sai em `OracleTools2026\Instalador\` (ignorado no git).
