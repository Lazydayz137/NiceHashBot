@echo off
:: Launch Multi-Algorithm Arbitrage Monitor
:: Scans 5 algorithms and shows ranked profitability

echo.
echo ╔═══════════════════════════════════════════════════════════╗
echo ║     LAUNCHING MULTI-ALGORITHM ARBITRAGE MONITOR...        ║
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

:: Launch multi-algorithm monitor
cd NHB3\bin\Debug
start "Multi-Algorithm Monitor" NHB3.exe multi

echo.
echo Monitor launched in new window!
echo.
timeout /t 3
