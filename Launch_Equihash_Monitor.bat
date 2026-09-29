@echo off
:: Launch Equihash Specialized Monitor
:: Perfect for monitoring active MRR Equihash contracts

echo.
echo ╔═══════════════════════════════════════════════════════════╗
echo ║        LAUNCHING EQUIHASH SPECIALIZED MONITOR...          ║
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
    echo Please configure your API credentials before using the monitor.
    echo.
    pause
)

:: Launch Equihash monitor
cd NHB3\bin\Debug
start "Equihash Monitor" NHB3.exe equihash

echo.
echo Equihash Monitor launched in new window!
echo.
timeout /t 3
