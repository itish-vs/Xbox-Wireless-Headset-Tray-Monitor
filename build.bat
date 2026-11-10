@echo off
setlocal
cd /d "%~dp0"

echo Building XboxHeadsetTray for Windows x64...
dotnet publish .\XboxHeadsetTray.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\publish
if errorlevel 1 (
    echo.
    echo Build failed. Check that the .NET 10 SDK is installed and internet access is available for NuGet restore.
    pause
    exit /b 1
)
echo.
echo Build complete: %CD%\publish\XboxHeadsetTray.exe
pause
