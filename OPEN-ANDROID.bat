@echo off
echo ================================================
echo   TeknoSOS Android App - Opening in Android Studio
echo ================================================
echo.

set ANDROID_PROJECT=%~dp0Mobile\TeknoSOS.Android

:: Check common Android Studio paths
set AS_PATH=

if exist "%LOCALAPPDATA%\Programs\Android Studio\bin\studio64.exe" (
    set AS_PATH=%LOCALAPPDATA%\Programs\Android Studio\bin\studio64.exe
) else if exist "%ProgramFiles%\Android\Android Studio\bin\studio64.exe" (
    set AS_PATH=%ProgramFiles%\Android\Android Studio\bin\studio64.exe
) else if exist "C:\Program Files\Android\Android Studio\bin\studio64.exe" (
    set AS_PATH=C:\Program Files\Android\Android Studio\bin\studio64.exe
)

if "%AS_PATH%"=="" (
    echo Android Studio not found in common locations.
    echo.
    echo Please open this folder manually in Android Studio:
    echo %ANDROID_PROJECT%
    echo.
    explorer "%ANDROID_PROJECT%"
    goto :end
)

echo Found Android Studio: %AS_PATH%
echo Opening project...
start "" "%AS_PATH%" "%ANDROID_PROJECT%"

:end
echo.
echo ================================================
echo   Android project: %ANDROID_PROJECT%
echo   
echo   Build commands:
echo     ./gradlew assembleDebug    - Build debug APK
echo     ./gradlew assembleRelease  - Build release APK
echo ================================================
pause
