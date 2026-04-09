@echo off
REM ============================================================================
REM TeknoSOS - Start Script
REM Nis aplikacionin dhe e hap ne https://teknosos.app
REM ============================================================================

echo.
echo ========================================
echo    TeknoSOS WebApp
echo    https://teknosos.app
echo ========================================
echo.

cd /d "c:\Users\CAS\Desktop\TeknoSOS.WebApp"

REM Ndalo proceset e vjetra
echo Duke ndalur proceset e vjetra...
taskkill /f /im cloudflared.exe >nul 2>&1
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5050" ^| findstr "LISTENING"') do taskkill /f /pid %%a >nul 2>&1

REM Starto aplikacionin .NET
echo Duke nisur TeknoSOS aplikacionin...
start /B dotnet run --launch-profile http

REM Prit qe app te startohet
echo Duke pritur 10 sekonda...
timeout /t 10 /nobreak > nul

REM Starto Cloudflare Tunnel
echo Duke nisur Cloudflare Tunnel...
start /B C:\cloudflared\cloudflared.exe tunnel run teknosos

timeout /t 3 /nobreak > nul

echo.
echo ========================================
echo   GATI! TeknoSOS eshte ONLINE
echo ========================================
echo.
echo   URL: https://teknosos.app
echo.
echo   Lokale: http://localhost:5050
echo.
echo ========================================
echo.
echo Mos e mbyll kete dritare!
echo.

REM Mbaj dritaren hapur
pause
