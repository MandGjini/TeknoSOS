@echo off
echo ================================================
echo   TeknoSOS iOS App Information
echo ================================================
echo.
echo iOS development requires macOS and Xcode.
echo.
echo Project location: %~dp0Mobile\TeknoSOS.iOS\
echo Xcode project:    TeknoSOS.xcodeproj
echo.
echo ------------------------------------------------
echo   INSTRUCTIONS FOR MAC
echo ------------------------------------------------
echo.
echo 1. Copy the Mobile/TeknoSOS.iOS folder to your Mac
echo 2. Open Terminal and navigate to the folder
echo 3. Run: open TeknoSOS.xcodeproj
echo 4. In Xcode: Product ^> Build (Cmd+B)
echo.
echo ------------------------------------------------
echo   REQUIREMENTS
echo ------------------------------------------------
echo - macOS Ventura or later
echo - Xcode 15.0+
echo - iOS 16.0+ deployment target
echo - Apple Developer account (for device testing)
echo.
echo ================================================
echo.
echo Opening iOS project folder...
explorer "%~dp0Mobile\TeknoSOS.iOS"
pause
