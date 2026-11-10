[Languages]
Name: "brazilian"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Setup]
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; AppId mantém o identificador das versões anteriores (o padrão do Inno Setup é o AppName); sem ele, a troca do nome
; faria a instalação aparecer como outro programa em "Aplicativos instalados".
AppId=Oracle Tools
AppName=Oracle Tools 2026
AppVersion=2.0.0
AppPublisher=Filipe Kanitz
DefaultDirName={commonpf}\OracleTools2026
DefaultGroupName=OracleTools2026
OutputDir=.
SetupIconFile="..\..\OracleTools.ico"
UninstallDisplayIcon={app}\OracleTools2026.exe
OutputBaseFilename=OracleTools2026-2.0.0
Compression=lzma
SolidCompression=yes
DisableProgramGroupPage=yes
; Os scripts de lock vão para o %APPDATA% do usuário que executa a instalação (intencional)
UsedUserAreasWarning=no

[Files]
; Copia todos os arquivos da pasta Release (inclui as dependências NuGet e o Windows App SDK) para a pasta de instalação
Source: "..\bin\Release\net10.0-windows10.0.26100.0\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Scripts do Monitor de Locks em %APPDATA%\OracleTools2026; não sobrescreve versões editadas pelo usuário e não remove na desinstalação
Source: "AppData\*"; DestDir: "{userappdata}\OracleTools2026"; Flags: onlyifdoesntexist uninsneveruninstall

[Icons]
Name: "{group}\Oracle Tools 2026"; Filename: "{app}\OracleTools2026.exe"
Name: "{commondesktop}\Oracle Tools 2026"; Filename: "{app}\OracleTools2026.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Opções adicionais:"

[Run]
; Executa o programa após a instalação
Filename: "{app}\OracleTools2026.exe"; Description: "Executar o Oracle Tools 2026"; Flags: nowait postinstall skipifsilent

[InstallDelete]
; Atalhos com o nome anterior ("Oracle Tools")
Type: files; Name: "{group}\Oracle Tools.lnk"
Type: files; Name: "{commondesktop}\Oracle Tools.lnk"
