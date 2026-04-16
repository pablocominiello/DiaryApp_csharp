@echo off
echo ========================================
echo  LIMPIEZA NUCLEAR - DiaryApp
echo ========================================
echo.

cd C:\Fuentes\VS\DiaryApp_csharp\DiaryApp_csharp

REM Cerrar Visual Studio
echo Cerrando Visual Studio...
taskkill /f /im devenv.exe 2>nul
timeout /t 3 >nul

REM Eliminar TODAS las carpetas bin/obj
echo Eliminando bin/obj de TODOS los proyectos...
for /d /r . %%d in (bin) do @if exist "%%d" rd /s /q "%%d" 2>nul
for /d /r . %%d in (obj) do @if exist "%%d" rd /s /q "%%d" 2>nul

REM Eliminar cache de Xamarin/Android
echo Eliminando cache de Xamarin...
rd /s /q "%LOCALAPPDATA%\Xamarin" 2>nul
rd /s /q "%TEMP%\Xamarin" 2>nul
rd /s /q "%USERPROFILE%\.nuget\packages\microsoft.android.sdk.buildtools" 2>nul

REM Limpiar cache de NuGet
echo Limpiando cache de NuGet...
dotnet nuget locals all --clear

REM Restaurar SOLUCIÓN COMPLETA
echo.
echo Restaurando solucion...
dotnet restore DiaryApp_csharp.sln

REM Compilar DiaryApp.Shared primero
echo.
echo Compilando DiaryApp.Shared...
dotnet build DiaryApp.Shared\DiaryApp.Shared.csproj -c Release

REM Compilar DiaryApp.Mobile
echo.
echo Compilando DiaryApp.Mobile para Android...
dotnet build DiaryApp.Mobile\DiaryApp.Mobile.csproj -c Release -f net9.0-android /p:AndroidSdkBuildToolsVersion=34.0.0

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo  COMPILACION EXITOSA!
    echo ========================================
    echo.
    echo Presiona cualquier tecla para generar APK...
    pause
    
    set /p PASSWORD=Ingresa password del keystore: 
    cd DiaryApp.Mobile
    dotnet publish -f net9.0-android -c Release /p:AndroidSigningPassword=%PASSWORD% /p:AndroidSdkBuildToolsVersion=34.0.0
    
    copy /Y "bin\Release\net9.0-android\publish\com.companyname.diaryapp.mobile-Signed.apk" "..\DiaryApp\wwwroot\downloads\DiaryApp.apk"
    cd ..
) else (
    echo.
    echo ========================================
    echo  ERROR EN LA COMPILACION
    echo ========================================
)

pause