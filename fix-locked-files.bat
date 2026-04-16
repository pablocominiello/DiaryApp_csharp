@echo off
echo ========================================
echo  Liberando archivos bloqueados
echo ========================================
echo.

REM Matar TODOS los procesos relacionados con build
echo Cerrando procesos de Visual Studio y MSBuild...
taskkill /f /im devenv.exe 2>nul
taskkill /f /im MSBuild.exe 2>nul
taskkill /f /im dotnet.exe 2>nul
taskkill /f /im VBCSCompiler.exe 2>nul

timeout /t 3 >nul

echo.
echo Limpiando cache de NuGet problemático...
rd /s /q "%USERPROFILE%\.nuget\packages\microsoft.windowsappsdk" 2>nul
rd /s /q "%USERPROFILE%\.nuget\packages\microsoft.ui.xaml" 2>nul

echo.
echo Intentando build de nuevo...
cd C:\Fuentes\VS\DiaryApp_csharp\DiaryApp_csharp

dotnet restore DiaryApp.Mobile\DiaryApp.Mobile.csproj

dotnet build DiaryApp.Mobile\DiaryApp.Mobile.csproj -c Release -f net9.0-android /p:AndroidSdkBuildToolsVersion=34.0.0

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo  BUILD EXITOSO!
    echo ========================================
    pause
) else (
    echo.
    echo ========================================
    echo  ERROR - Reinicia Windows
    echo ========================================
    pause
)