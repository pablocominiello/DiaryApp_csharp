@echo off
setlocal enabledelayedexpansion

echo ==================================================
echo  Limpieza Total y Compilacion - DiaryApp Mobile
echo ==================================================
echo.

REM Cerrar Visual Studio
echo [1/8] Cerrando Visual Studio...
taskkill /f /im devenv.exe 2>nul
timeout /t 2 >nul

REM Eliminar bin y obj recursivamente
echo [2/8] Eliminando carpetas bin y obj...
for /d /r . %%d in (bin) do @if exist "%%d" (
    echo Eliminando: %%d
    rd /s /q "%%d" 2>nul
)
for /d /r . %%d in (obj) do @if exist "%%d" (
    echo Eliminando: %%d
    rd /s /q "%%d" 2>nul
)

REM Limpiar archivos temporales
echo [3/8] Limpiando archivos temporales...
del /s /q *.suo 2>nul
del /s /q *.user 2>nul

REM Limpiar cache de NuGet
echo [4/8] Limpiando cache de NuGet...
dotnet nuget locals temp-cache --clear

REM Restaurar solución
echo.
echo [5/8] Restaurando solucion...
dotnet restore
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la restauracion
    pause
    exit /b 1
)

REM Compilar DiaryApp.Shared
echo.
echo [6/8] Compilando DiaryApp.Shared...
dotnet build DiaryApp.Shared\DiaryApp.Shared.csproj -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Fallo DiaryApp.Shared
    pause
    exit /b 1
)

REM Compilar DiaryApp.Mobile para Android
echo.
echo [7/8] Compilando DiaryApp.Mobile para Android...
dotnet build DiaryApp.Mobile\DiaryApp.Mobile.csproj -c Release -f net9.0-android
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la compilacion de Android
    pause
    exit /b 1
)

REM Publicar APK
echo.
echo [8/8] Ingresa la contrasena del keystore:
set /p PASSWORD=Password: 

echo.
echo Generando APK firmado...
cd DiaryApp.Mobile
dotnet publish -f net9.0-android -c Release /p:AndroidSigningPassword=%PASSWORD%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la generacion del APK
    cd ..
    pause
    exit /b 1
)

echo.
echo Copiando APK a wwwroot...
copy /Y "bin\Release\net9.0-android\publish\com.companyname.diaryapp.mobile-Signed.apk" "..\DiaryApp\wwwroot\downloads\DiaryApp.apk"

if %errorlevel% equ 0 (
    echo.
    echo ==================================================
    echo  APK generado exitosamente!
    echo ==================================================
    dir "..\DiaryApp\wwwroot\downloads\DiaryApp.apk"
    echo ==================================================
) else (
    echo [ERROR] No se pudo copiar el APK
)

cd ..
pause