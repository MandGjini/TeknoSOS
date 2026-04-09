@echo off
echo ================================================
echo   TeknoSOS Web App - Opening in VS Code
echo ================================================
echo.

:: Check if VS Code is installed
where code >nul 2>&1
if %errorlevel% neq 0 (
    echo VS Code not found. Trying Visual Studio...
    if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" (
        start "" "%ProgramFiles%\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" "%~dp0TeknoSOS.WebApp.slnx"
    ) else (
        echo Please open TeknoSOS.WebApp.slnx manually
        explorer "%~dp0"
    )
    goto :end
)

echo Opening project in VS Code...
code "%~dp0"

:end
echo.
echo ================================================
echo   Web project folder: %~dp0
echo   Solution file: TeknoSOS.WebApp.slnx
echo ================================================
pause
