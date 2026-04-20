@echo off
setlocal enabledelayedexpansion
echo ==================================================
echo  Generador de APK - DiaryApp Mobile (SAFE)
echo ==================================================
echo.

cd DiaryApp.Mobile

echo [1/4] Limpiando proyecto...
dotnet clean -c Release -f net9.0-android
if exist "bin\Release\net9.0-android" rd /s /q "bin\Release\net9.0-android"
if exist "obj\Release\net9.0-android" rd /s /q "obj\Release\net9.0-android"

echo.
echo [2/4] Restaurando dependencias...
dotnet restore
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la restauracion
    cd ..
    pause
    exit /b 1
)

echo.
echo [3/4] Ingresa la contrasena del keystore:
set /p PASSWORD=Password: 

echo.
echo [4/4] Generando APK firmado (esto puede tardar 2-3 minutos)...
dotnet publish -f net9.0-android -c Release ^
    /p:AndroidSigningPassword=%PASSWORD% ^
    /p:AndroidPackageFormat=apk ^
    /p:AndroidKeyStore=true ^
    /p:AndroidSigningKeyStore=diaryapp.keystore ^
    /p:AndroidSigningKeyAlias=diaryapp ^
    /p:AndroidSigningStorePass=%PASSWORD% ^
    /p:AndroidSigningKeyPass=%PASSWORD%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la generacion del APK
    echo.
    echo Si el error es XAGJS7001, ejecuta:
    echo   dotnet workload update
    cd ..
    pause
    exit /b 1
)

echo.
echo [FINAL] Copiando APK a wwwroot...
set APK_SOURCE=bin\Release\net9.0-android\publish\com.companyname.diaryapp.mobile-Signed.apk
set APK_DEST=..\DiaryApp\wwwroot\downloads\DiaryApp.apk

if exist "%APK_SOURCE%" (
    copy /Y "%APK_SOURCE%" "%APK_DEST%"
    if %errorlevel% equ 0 (
        echo.
        echo ==================================================
        echo  APK generado exitosamente!
        echo ==================================================
        for %%A in ("%APK_DEST%") do set size=%%~zA
        set /a sizeMB=!size! / 1048576
        echo  Tamano: !sizeMB! MB
        echo  Ubicacion: DiaryApp\wwwroot\downloads\DiaryApp.apk
        echo ==================================================
        dir "%APK_DEST%"
    ) else (
        echo [ERROR] No se pudo copiar el APK
    )
) else (
    echo [ERROR] No se encontro el APK en: %APK_SOURCE%
)

cd ..
pause