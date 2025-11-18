@echo off
:: Launch NiceHashBot GUI
:: Modern dashboard with arbitrage monitoring

echo.
echo ╔═══════════════════════════════════════════════════════════╗
echo ║           LAUNCHING NICEHASHBOT v2.0 GUI...               ║
echo ╚═══════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

if not exist "NHB3\bin\Debug\NHB3.exe" (
    echo ERROR: NHB3.exe not found!
    echo.
    echo Please build the project first:
    echo   1. Open NHB3.sln in Visual Studio
    echo   2. Build ^(F7^)
    echo   3. Run this launcher again
    echo.
    pause
    exit /b 1
)

:: Check for configuration files
if not exist "NHB3\bin\Debug\settings.json" (
    echo WARNING: settings.json not found!
    echo.
    echo First-time setup required:
    echo   1. Copy NHB3\bin\Debug\settings.json.template to settings.json
    echo   2. Edit settings.json with your API credentials
    echo   3. Copy bot.json.template to bot.json
    echo   4. Edit bot.json with your Mining-Dutch BTC address
    echo.
    pause
)

:: Launch GUI
cd NHB3\bin\Debug
start "" NHB3.exe

echo.
echo GUI launched!
echo.
timeout /t 2
