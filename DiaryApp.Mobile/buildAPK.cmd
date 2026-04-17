@echo off
setlocal enabledelayedexpansion
echo ==================================================
echo  Generador de APK Android - 9Julio App
echo ==================================================
echo.

REM Verificar que estamos en la carpeta correcta
if not exist "DiaryApp.Mobile.csproj" (
    echo [ERROR] Este script debe ejecutarse desde la carpeta DiaryApp.Mobile
    echo Ubicacion actual: %CD%
    pause
    exit /b 1
)

REM Detectar y cerrar Visual Studio
echo [INFO] Verificando procesos de Visual Studio...
tasklist /FI "IMAGENAME eq devenv.exe" 2>NUL | find /I /N "devenv.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [WARN] Visual Studio esta en ejecucion
    echo.
    set /p CLOSE_VS="Deseas cerrar Visual Studio automaticamente? (s/n): "
    if /i "!CLOSE_VS!"=="s" (
        echo [INFO] Cerrando Visual Studio...
        taskkill /F /IM devenv.exe >nul 2>&1
        timeout /t 3 >nul
        echo [OK] Visual Studio cerrado
    ) else (
        echo.
        echo [ERROR] Debes cerrar Visual Studio manualmente antes de continuar
        pause
        exit /b 1
    )
) else (
    echo [OK] Visual Studio no esta en ejecucion
)
echo.

REM Verificar keystore
echo [INFO] Verificando keystore...
set KEYSTORE_NAME=diaryapp.keystore
set KEYSTORE_ALIAS=9julio

if not exist "%KEYSTORE_NAME%" (
    echo [ERROR] No se encontro: %KEYSTORE_NAME%
    echo.
    echo Para crear el keystore ejecuta:
    echo   keytool -genkey -v -keystore %KEYSTORE_NAME% -alias %KEYSTORE_ALIAS% -keyalg RSA -keysize 2048 -validity 10000
    echo.
    pause
    exit /b 1
)

echo [OK] Keystore encontrado: %KEYSTORE_NAME%
echo [OK] Alias configurado: %KEYSTORE_ALIAS%
echo.

REM Hardcodear la contraseña temporalmente para debug
set PASSWORD=Dragon6973

echo [INFO] Usando contraseña guardada...
echo.

echo [1/8] Validando keystore...
keytool -list -keystore "%KEYSTORE_NAME%" -storepass "%PASSWORD%" -alias "%KEYSTORE_ALIAS%" 2>&1 | find "9julio" >nul

if %errorlevel% neq 0 (
    echo [ERROR] Validacion fallida
    echo.
    echo Mostrando contenido del keystore:
    keytool -list -keystore "%KEYSTORE_NAME%" -storepass "%PASSWORD%"
    echo.
    pause
    exit /b 1
)

echo [OK] Keystore validado correctamente (alias: %KEYSTORE_ALIAS%)
echo.

echo [2/8] Limpieza COMPLETA del proyecto...
echo [INFO] Esperando a que se liberen archivos bloqueados...
timeout /t 2 >nul

dotnet clean -c Release >nul 2>&1
dotnet clean -c Debug >nul 2>&1

REM Eliminar carpetas de cache
set MAX_RETRIES=3
set RETRY_COUNT=0

:RETRY_DELETE
if exist "bin" (
    rd /s /q "bin" 2>nul
    if exist "bin" (
        set /a RETRY_COUNT+=1
        if !RETRY_COUNT! leq %MAX_RETRIES% (
            echo [WARN] Archivos bloqueados, reintentando... !RETRY_COUNT!/%MAX_RETRIES%
            timeout /t 2 >nul
            goto RETRY_DELETE
        ) else (
            echo [ERROR] No se pudo eliminar la carpeta bin
            pause
            exit /b 1
        )
    )
)

set RETRY_COUNT=0
:RETRY_DELETE_OBJ
if exist "obj" (
    rd /s /q "obj" 2>nul
    if exist "obj" (
        set /a RETRY_COUNT+=1
        if !RETRY_COUNT! leq %MAX_RETRIES% (
            echo [WARN] Archivos bloqueados en obj, reintentando... !RETRY_COUNT!/%MAX_RETRIES%
            timeout /t 2 >nul
            goto RETRY_DELETE_OBJ
        ) else (
            echo [ERROR] No se pudo eliminar la carpeta obj
            pause
            exit /b 1
        )
    )
)

echo [OK] Limpieza completa
echo.

echo [3/8] Restaurando dependencias...
dotnet restore DiaryApp.Mobile.csproj
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la restauracion
    pause
    exit /b 1
)

echo.
echo [4/8] Compilando DiaryApp.Shared...
cd ..\DiaryApp.Shared
dotnet build DiaryApp.Shared.csproj -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Fallo DiaryApp.Shared
    cd ..\DiaryApp.Mobile
    pause
    exit /b 1
)
cd ..\DiaryApp.Mobile

echo.
echo [5/8] Compilando para Android SIN firmar (Debug)...
dotnet build DiaryApp.Mobile.csproj -c Debug -f net9.0-android
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la compilacion Debug
    pause
    exit /b 1
)

echo.
echo [6/8] Compilando para Android CON firma (Release)...
echo [INFO] Keystore: %KEYSTORE_NAME%
echo [INFO] Alias:    %KEYSTORE_ALIAS%
echo.
dotnet build DiaryApp.Mobile.csproj -c Release -f net9.0-android ^
    /p:AndroidKeyStore=true ^
    /p:AndroidSigningKeyStore=%KEYSTORE_NAME% ^
    /p:AndroidSigningKeyAlias=%KEYSTORE_ALIAS% ^
    /p:AndroidSigningKeyPass=%PASSWORD% ^
    /p:AndroidSigningStorePass=%PASSWORD%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la compilacion Release
    pause
    exit /b 1
)

echo.
echo [7/8] Generando APK firmado con publish...
dotnet publish DiaryApp.Mobile.csproj -f net9.0-android -c Release ^
    /p:AndroidKeyStore=true ^
    /p:AndroidSigningKeyStore=%KEYSTORE_NAME% ^
    /p:AndroidSigningKeyAlias=%KEYSTORE_ALIAS% ^
    /p:AndroidSigningKeyPass=%PASSWORD% ^
    /p:AndroidSigningStorePass=%PASSWORD%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo el publish
    pause
    exit /b 1
)

echo.
echo [8/8] Buscando y validando APK...

REM Buscar APK firmado
set APK_SOURCE=bin\Release\net9.0-android\publish\com.circulo9julio.app-Signed.apk
set APK_NEW_ID=0

if exist "%APK_SOURCE%" (
    set APK_NEW_ID=1
    echo [OK] APK con NUEVO ID encontrado
) else (
    set APK_SOURCE=bin\Release\net9.0-android\publish\com.companyname.diaryapp.mobile-Signed.apk
    
    if exist "!APK_SOURCE!" (
        echo [OK] APK con ID ANTIGUO encontrado
    ) else (
        echo [ERROR] No se encontro APK firmado
        echo.
        echo [INFO] APKs generados:
        dir /b bin\Release\net9.0-android\publish\*.apk 2>nul
        pause
        exit /b 1
    )
)

echo.
echo [INFO] Validando firma del APK...
jarsigner -verify "%APK_SOURCE%" 2>nul | findstr /C:"jar verified"

if %errorlevel% neq 0 (
    echo [WARN] No se pudo verificar la firma con jarsigner
    echo [INFO] Esto es normal si no tienes Java JDK completo instalado
    echo.
)

REM Copiar APK
set APK_DEST=..\DiaryApp\wwwroot\downloads\9JulioApp.apk

if not exist "..\DiaryApp\wwwroot\downloads" (
    mkdir "..\DiaryApp\wwwroot\downloads"
)

echo [INFO] Copiando APK...
copy /Y "%APK_SOURCE%" "%APK_DEST%"

if %errorlevel% equ 0 (
    echo.
    echo ==================================================
    echo  APK GENERADO EXITOSAMENTE
    echo ==================================================
    
    for %%A in ("%APK_DEST%") do set size=%%~zA
    set /a sizeMB=!size! / 1048576
    
    echo.
    echo  Archivo:      9JulioApp.apk
    echo  Tamano:       !sizeMB! MB
    echo  Ubicacion:    DiaryApp\wwwroot\downloads\
    echo  Keystore:     %KEYSTORE_NAME%
    echo  Alias:        %KEYSTORE_ALIAS%
    
    if %APK_NEW_ID%==1 (
        echo  Package ID:   com.circulo9julio.app [NUEVO]
    ) else (
        echo  Package ID:   com.companyname.diaryapp.mobile [ANTIGUO]
    )
    
    echo.
    echo ==================================================
    echo  INSTALACION
    echo ==================================================
    echo.
    
    if %APK_NEW_ID%==1 (
        echo  [!] Package ID NUEVO - Requiere DESINSTALAR app anterior
        echo.
        echo  Pasos:
        echo  1. Desinstala la app actual de tu celular/emulador
        echo  2. Copia/transfiere 9JulioApp.apk
        echo  3. Instala el APK
        echo  4. Los datos locales se perderan
    ) else (
        echo  [OK] Package ID ANTIGUO - Actualiza sin desinstalar
        echo.
        echo  Pasos:
        echo  1. Copia/transfiere 9JulioApp.apk
        echo  2. Instala (sobreescribe la app existente)
        echo  3. Los datos locales se conservan
    )
    
    echo.
    echo ==================================================
    
    dir "%APK_DEST%" | findstr /i "9JulioApp"
    
) else (
    echo [ERROR] No se pudo copiar el APK
    pause
    exit /b 1
)

echo.
echo ==================================================
echo  RECORDATORIO IMPORTANTE
echo ==================================================
echo.
echo  Credenciales del Keystore:
echo  - Archivo: diaryapp.keystore
echo  - Alias:   9julio  
echo  - Password: Dragon6973
echo.
echo  GUARDA ESTA INFORMACION EN UN LUGAR SEGURO
echo  Sin este keystore NO podras actualizar la app
echo ==================================================
echo.
pause