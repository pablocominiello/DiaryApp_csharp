# Script para crear un NUEVO keystore y configurar el proyecto
$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🔐 Creador de Nuevo Keystore - 9Julio                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$projectPath = "DiaryApp.Mobile"
$keystoreName = "9julio.keystore"
$keystorePath = "$projectPath\$keystoreName"
$alias = "9julio"

# Verificar si ya existe
if (Test-Path $keystorePath) {
    Write-Host "⚠️  Ya existe un keystore en: $keystorePath" -ForegroundColor Yellow
    $overwrite = Read-Host "¿Deseas crear uno nuevo? (s/n)"
    if ($overwrite -ne "s") {
        Write-Host "❌ Operación cancelada" -ForegroundColor Red
        exit 0
    }
    Remove-Item $keystorePath -Force
    Write-Host "✅ Keystore anterior eliminado" -ForegroundColor Green
}

Write-Host ""
Write-Host "📝 Información para el certificado:" -ForegroundColor Cyan
Write-Host ""

# Solicitar datos
Write-Host "🔑 Contraseña del keystore (¡GUÁRDALA BIEN!): " -ForegroundColor Yellow
$securePassword = Read-Host -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))

Write-Host "🔑 Confirma la contraseña: " -ForegroundColor Yellow
$securePassword2 = Read-Host -AsSecureString
$password2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword2))

if ($password -ne $password2) {
    Write-Host "❌ Las contraseñas no coinciden" -ForegroundColor Red
    exit 1
}

Write-Host ""
$firstName = Read-Host "Nombre"
$lastName = Read-Host "Apellido"
$organization = Read-Host "Organización (ej: Circulo 9 de Julio)"
$city = Read-Host "Ciudad"
$state = Read-Host "Provincia/Estado"
$country = Read-Host "Código de País (ej: AR)"

$dname = "CN=$firstName $lastName, OU=$organization, O=$organization, L=$city, ST=$state, C=$country"

Write-Host ""
Write-Host "🔨 Generando nuevo keystore..." -ForegroundColor Cyan
Write-Host ""

try {
    # Cambiar al directorio del proyecto
    Push-Location $projectPath
    
    # Generar keystore
    $keytoolArgs = @(
        "-genkey",
        "-v",
        "-keystore", $keystoreName,
        "-alias", $alias,
        "-keyalg", "RSA",
        "-keysize", "2048",
        "-validity", "10000",
        "-storepass", $password,
        "-keypass", $password,
        "-dname", $dname
    )
    
    & keytool @keytoolArgs
    
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║  ✅ KEYSTORE CREADO EXITOSAMENTE                      ║" -ForegroundColor Green
        Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "📋 Información del Keystore:" -ForegroundColor Cyan
        Write-Host "   Archivo:  $keystorePath" -ForegroundColor Gray
        Write-Host "   Alias:    $alias" -ForegroundColor Gray
        Write-Host "   Válido:   ~27 años (10,000 días)" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "⚠️  IMPORTANTE - GUARDA ESTA INFORMACIÓN:" -ForegroundColor Red
        Write-Host "┌─────────────────────────────────────────────────────┐" -ForegroundColor Yellow
        Write-Host "│ Contraseña: [LA QUE INGRESASTE]                    │" -ForegroundColor Yellow
        Write-Host "│ Alias:      $alias                                    │" -ForegroundColor Yellow
        Write-Host "│ Archivo:    $keystoreName                           │" -ForegroundColor Yellow
        Write-Host "└─────────────────────────────────────────────────────┘" -ForegroundColor Yellow
        Write-Host ""
        
        Write-Host "📝 Ahora actualizaré el archivo .csproj..." -ForegroundColor Cyan
        
        # Actualizar .csproj
        $csprojPath = "$projectPath\DiaryApp.Mobile.csproj"
        $csprojContent = Get-Content $csprojPath -Raw
        
        # Cambiar ApplicationId para que sea una app DIFERENTE
        $newAppId = "com.circulo9julio.app"
        $csprojContent = $csprojContent -replace '<ApplicationId>.*</ApplicationId>', "<ApplicationId>$newAppId</ApplicationId>"
        
        # Cambiar ApplicationTitle
        $csprojContent = $csprojContent -replace '<ApplicationTitle>.*</ApplicationTitle>', '<ApplicationTitle>9Julio</ApplicationTitle>'
        
        # Actualizar configuración del keystore
        $csprojContent = $csprojContent -replace '<AndroidSigningKeyStore>.*</AndroidSigningKeyStore>', "<AndroidSigningKeyStore>$keystoreName</AndroidSigningKeyStore>"
        $csprojContent = $csprojContent -replace '<AndroidSigningKeyAlias>.*</AndroidSigningKeyAlias>', "<AndroidSigningKeyAlias>$alias</AndroidSigningKeyAlias>"
        
        # Guardar cambios
        Set-Content $csprojPath -Value $csprojContent -NoNewline
        
        Write-Host "✅ Archivo .csproj actualizado" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "🎯 Próximos pasos:" -ForegroundColor Cyan
        Write-Host "   1️⃣  Desinstala la app actual de tu celular" -ForegroundColor Yellow
        Write-Host "   2️⃣  Ejecuta: .\buildAPK.cmd" -ForegroundColor Gray
        Write-Host "   3️⃣  Ingresa la contraseña que guardaste" -ForegroundColor Gray
        Write-Host "   4️⃣  Instala el nuevo APK firmado" -ForegroundColor Gray
        Write-Host ""
        Write-Host "⚠️  CRÍTICO: Como cambió el keystore, es una app NUEVA" -ForegroundColor Red
        Write-Host "   - Los usuarios deben DESINSTALAR la anterior" -ForegroundColor Red
        Write-Host "   - Se perderán los datos locales" -ForegroundColor Red
        Write-Host ""
        
        # Crear archivo de recordatorio
        $reminderPath = "$projectPath\KEYSTORE_INFO.txt"
        @"
╔════════════════════════════════════════════════════════╗
║  INFORMACIÓN DEL KEYSTORE - NO COMPARTIR             ║
╚════════════════════════════════════════════════════════╝

Creado: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Archivo:     $keystoreName
Alias:       $alias
Contraseña:  [COMPLETAR MANUALMENTE]

ApplicationId: $newAppId

⚠️  IMPORTANTE:
- Guarda este archivo en un lugar SEGURO
- Haz backup del archivo .keystore
- Sin este keystore NO podrás actualizar la app
- Perderlo significa crear una app NUEVA

📍 Ubicación del keystore:
$keystorePath
"@ | Out-File -FilePath $reminderPath -Encoding UTF8
        
        Write-Host "💾 Archivo de recordatorio creado: $reminderPath" -ForegroundColor Green
        Write-Host ""
        
    } else {
        throw "Error al generar keystore"
    }
    
} catch {
    Write-Host ""
    Write-Host "❌ ERROR al crear keystore:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Verifica que tengas Java JDK instalado:" -ForegroundColor Cyan
    Write-Host "   java -version" -ForegroundColor Gray
    Write-Host "   keytool" -ForegroundColor Gray
    exit 1
}

Write-Host "🎉 Proceso completado" -ForegroundColor Green