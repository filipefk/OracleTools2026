# OracleTools2026

Conjunto de utilitários para administração e gestão de mudanças em bancos de dados Oracle.

É a reescrita completa do [OracleTools](../OracleTools) original (.NET Framework 2.0, x86), agora em **.NET 10**, **x64**, com driver Oracle 100% gerenciado e práticas atuais de desenvolvimento. As funcionalidades estão sendo migradas aos poucos.

## Tecnologias

| Item | Detalhe |
|------|---------|
| Linguagem | C# (nullable habilitado) |
| Interface | Windows Forms (`net10.0-windows`), High DPI `PerMonitorV2` |
| Plataforma | x64 |
| Acesso ao Oracle | [Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core) (não exige Oracle Client instalado) |
| Injeção de dependência | `Microsoft.Extensions.Hosting` (Generic Host) |
| Preferências | Registro do Windows (`HKCU\Software\ORACLE_Tools\Parametros`) |

## Estrutura do projeto

```
OracleTools2026.slnx
Release.cmd                 # Compila em Release e gera o instalador
OracleTools2026/
├── Instalador/Instalador.iss  # Script do Inno Setup
├── Program.cs              # Host + DI, roteamento por argumento de linha de comando
├── Models/                 # Records de domínio (DatabaseObject, ConnectionInfo, DdlExtractionResult...)
├── Settings/               # Preferências no registro, conexões salvas e criptografia compatível com o original
├── Data/                   # Acesso ao Oracle: sessão, listagens, extração de DDL, recompilação, tnsnames
├── Services/               # Saída da DDL: arquivos, área de transferência, editor de texto
└── UI/                     # Formulários (ExtractDdlForm, LoginForm, SettingsForm) e utilitários de tela
```

## Funcionalidades

### Extrair DDL de objetos de banco (`ExtractDdlForm`)
- Lista objetos filtrando por owner, tipo e parte do nome.
- Lista as **últimas compilações** (objetos alterados nos últimos N dias) e os **objetos inválidos**, com as mensagens de erro de compilação.
- **Recompila objetos inválidos** em tentativas sucessivas (types, packages, procedures, functions, triggers, views e sinônimos) e gera um relatório do que foi compilado.
- Extrai a DDL de procedures, functions, packages (spec + body), triggers (inclusive `DISABLE`), types (spec + body), views, sequences, jobs do `DBMS_SCHEDULER`, Java sources, sinônimos e grants.
- Extrai dos **objetos marcados**, de **toda a lista** ou de uma **lista digitada** (`OWNER.NOME`, um por linha).
- Destino: **arquivos** numa pasta (`Owner\Tipo\OWNER.NOME.ext`, em UTF-8), **área de transferência** ou **editor de texto** padrão do Windows.
- Opções de incluir um **cabeçalho** (dados da conexão, datas e status do objeto) e as **mensagens de erro de compilação** nas linhas correspondentes.
- Se existirem as tabelas de auditoria do schema `AESPROD`, mostra o último alterador de cada objeto e inclui os dados da auditoria no cabeçalho.
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

### Ainda não migradas
Extrair script de usuários, detalhes de usuário e de role, controle de concorrência, backups de objetos, alerta baseado em consulta, procura de locks, sessões ativas, jobs, DBLINKs, agendamento de scripts, informações do banco e menu principal.

## Execução por linha de comando

```
OracleTools2026.exe <FERRAMENTA> [usuario senha alias_tns]
```

| Argumento | Ferramenta |
|-----------|------------|
| `EXTRACT_DDL_SOURCE` | Extrair DDL de objetos |

Sem argumento ou com um argumento desconhecido, abre o Extrair DDL (até o menu principal ser migrado). Com usuário, senha e alias, a tela já abre conectada.

## Conexão e preferências

- A conexão usa os aliases do **`tnsnames.ora`**, cujo caminho o usuário seleciona na primeira vez (a pasta do arquivo é usada como `TNS_ADMIN` do driver).
- As preferências ficam no registro em `HKEY_CURRENT_USER\Software\ORACLE_Tools\Parametros`, **as mesmas chaves do OracleTools original**: caminho do tnsnames, último login, pasta padrão de extração, opções de cabeçalho e de mensagens de erro, conexões salvas (encriptadas, no mesmo formato do original).
- Os parâmetros podem ser editados pelo menu **Arquivo → Parâmetros**.

## Requisitos

- Windows x64.
- .NET 10 Desktop Runtime.
- Acesso de rede ao banco Oracle e um `tnsnames.ora` com os aliases.

## Compilação

Abra `OracleTools2026.slnx` no Visual Studio 2026 (ou superior) e compile. Os pacotes NuGet são restaurados automaticamente.

```bash
dotnet build OracleTools2026.slnx -c Release
```

## Instalador

Instalador para Windows x64 gerado com [Inno Setup](https://jrsoftware.org/isinfo.php). Instala em `C:\Program Files\OracleTools2026`, cria atalho no menu Iniciar (e, opcionalmente, na área de trabalho). A desinstalação remove tudo isso; as preferências no registro (`HKCU\Software\ORACLE_Tools`) são mantidas, pois são compartilhadas com o OracleTools original.

O computador de destino precisa ter o **.NET 10 Desktop Runtime** instalado.

Para gerar (requer o `iscc` do Inno Setup no PATH):

1. Atualize a versão em `OracleTools2026\Instalador\Instalador.iss` (`AppVersion` e `OutputBaseFilename`) e em `OracleTools2026\OracleTools2026.csproj` (`AssemblyVersion`, `FileVersion` e `InformationalVersion`).
2. Execute `Release.cmd` na raiz: compila a solução em Release, apaga o instalador anterior e gera `OracleTools2026\Instalador\OracleTools2026-X.Y.Z.exe`.
