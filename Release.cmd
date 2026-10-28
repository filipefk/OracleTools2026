@echo off
setlocal

REM 1. Build completo da solução em modo Release
dotnet build OracleTools2026.slnx -c Release -p:Platform=x64
if errorlevel 1 (
    echo Erro ao compilar a solução.
	pause
    exit /b 1
)

REM 2. Excluir o arquivo do instalador anterior, se existir
if exist OracleTools2026\Instalador\OracleTools2026-*.exe (
    del /f /q OracleTools2026\Instalador\OracleTools2026-*.exe
)

REM 3. Buildar o instalador com o Inno Setup
iscc OracleTools2026\Instalador\Instalador.iss
if errorlevel 1 (
    echo Erro ao gerar o instalador.
	pause
    exit /b 1
)

echo Processo concluído com sucesso.
endlocal
pause
