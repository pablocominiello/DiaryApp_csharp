# Script para validar la contraseña del keystore actual
# NO modifica ni genera nada, solo valida

$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🔐 Validador de Keystore - DiaryApp Mobile          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Rutas del proyecto
$projectPath = "DiaryApp.Mobile"
$keystorePath = "$projectPath\diaryapp.keystore"

# Verificar que existe el keystore
if (-Not (Test-Path $keystorePath)) {
    Write-Host "❌ ERROR: No se encontró el keystore en:" -ForegroundColor Red
    Write-Host "   $keystorePath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Si quieres crear uno nuevo, usa el comando:" -ForegroundColor Cyan
    Write-Host "   keytool -genkey -v -keystore diaryapp.keystore -alias diaryapp -keyalg RSA -keysize 2048 -validity 10000" -ForegroundColor Gray
    exit 1
}

Write-Host "✅ Keystore encontrado: $keystorePath" -ForegroundColor Green
Write-Host ""

# Obtener información del keystore
$fileInfo = Get-Item $keystorePath
Write-Host "📊 Información del archivo:" -ForegroundColor Cyan
Write-Host "   Tamaño:        $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "   Creado:        $($fileInfo.CreationTime)" -ForegroundColor Gray
Write-Host "   Modificado:    $($fileInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

# Solicitar contraseña (sin mostrarla)
Write-Host "🔑 Ingresa la contraseña del keystore para validar:" -ForegroundColor Yellow
$securePassword = Read-Host "   Contraseña" -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))

Write-Host ""
Write-Host "🔍 Validando contraseña..." -ForegroundColor Cyan
Write-Host ""

# Intentar listar el contenido del keystore (si la contraseña es correcta, funcionará)
try {
    # keytool -list -v -keystore <path> -storepass <password>
    $keytoolOutput = & keytool -list -v -keystore $keystorePath -storepass $password 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║  ✅ CONTRASEÑA CORRECTA                               ║" -ForegroundColor Green
        Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        
        # Extraer información del alias
        if ($keytoolOutput -match "Alias name:\s*(.+)") {
            $alias = $matches[1].Trim()
            Write-Host "📋 Detalles del Keystore:" -ForegroundColor Cyan
            Write-Host "   Alias:         $alias" -ForegroundColor Gray
        }
        
        if ($keytoolOutput -match "Creation date:\s*(.+)") {
            $creationDate = $matches[1].Trim()
            Write-Host "   Fecha creación: $creationDate" -ForegroundColor Gray
        }
        
        if ($keytoolOutput -match "Valid from:.*until:\s*(.+)") {
            $validUntil = $matches[1].Trim()
            Write-Host "   Válido hasta:   $validUntil" -ForegroundColor Gray
        }
        
        if ($keytoolOutput -match "SHA1:\s*(.+)") {
            $sha1 = $matches[1].Trim()
            Write-Host "   SHA1:          $sha1" -ForegroundColor Gray
        }
        
        Write-Host ""
        Write-Host "💾 Configuración actual en DiaryApp.Mobile.csproj:" -ForegroundColor Cyan
        Write-Host "   AndroidSigningKeyStore = diaryapp.keystore" -ForegroundColor Gray
        Write-Host "   AndroidSigningKeyAlias = diaryapp" -ForegroundColor Gray
        Write-Host ""
        Write-Host "✅ Tu contraseña guardada es CORRECTA" -ForegroundColor Green
        Write-Host "✅ Puedes usarla para generar el APK firmado" -ForegroundColor Green
        Write-Host ""
        Write-Host "📝 Para cambiar solo el NOMBRE del APK sin problemas:" -ForegroundColor Yellow
        Write-Host "   1. Edita DiaryApp.Mobile.csproj" -ForegroundColor Gray
        Write-Host "   2. Cambia <ApplicationTitle>9Julio</ApplicationTitle>" -ForegroundColor Gray
        Write-Host "   3. Mantén el mismo <ApplicationId>" -ForegroundColor Gray
        Write-Host "   4. Mantén el mismo keystore y contraseña" -ForegroundColor Gray
        Write-Host ""
        Write-Host "⚠️  Si cambias ApplicationId O keystore, será una app DIFERENTE" -ForegroundColor Red
        Write-Host ""
        
    } else {
        throw "Error al validar keystore"
    }
    
} catch {
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ❌ CONTRASEÑA INCORRECTA                             ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Posibles causas:" -ForegroundColor Yellow
    Write-Host "   • La contraseña guardada es incorrecta" -ForegroundColor Gray
    Write-Host "   • El keystore está corrupto" -ForegroundColor Gray
    Write-Host "   • No tienes keytool instalado (viene con Java JDK)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🔧 Soluciones:" -ForegroundColor Cyan
    Write-Host "   Opción 1: Intenta recordar/recuperar la contraseña original" -ForegroundColor Gray
    Write-Host "   Opción 2: Genera un NUEVO keystore (requerirá desinstalar app)" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "🎉 Validación completada exitosamente" -ForegroundColor Green