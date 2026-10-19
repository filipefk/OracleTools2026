[Languages]
Name: "brazilian"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Setup]
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppName=Oracle Tools
AppVersion=1.0.0
AppPublisher=Filipe Kanitz
DefaultDirName={commonpf}\OracleTools2026
DefaultGroupName=OracleTools2026
OutputDir=.
SetupIconFile="..\..\OracleTools.ico"
UninstallDisplayIcon={app}\OracleTools2026.exe
OutputBaseFilename=OracleTools2026-1.0.0
Compression=lzma
SolidCompression=yes
DisableProgramGroupPage=yes

[Files]
; Copia todos os arquivos da pasta Release (inclui as dependências NuGet e runtimes) para a pasta de instalação
Source: "..\bin\Release\net10.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Oracle Tools"; Filename: "{app}\OracleTools2026.exe"
Name: "{commondesktop}\Oracle Tools"; Filename: "{app}\OracleTools2026.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Opções adicionais:"

[Run]
; Executa o programa após a instalação
Filename: "{app}\OracleTools2026.exe"; Description: "Executar o Oracle Tools"; Flags: nowait postinstall skipifsilent
