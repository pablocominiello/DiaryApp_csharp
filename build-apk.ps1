# Script para generar y copiar APK con nombre correcto
$ErrorActionPreference = "Stop"

$projectPath = "DiaryApp.Mobile"  # o "x9JulioApp" según tu carpeta
$outputApk = "$projectPath\bin\Release\net9.0-android\publish\com.companyname.diaryapp.mobile-Signed.apk"
$destinationWeb = "DiaryApp\wwwroot\downloads\DiaryApp.apk"

Write-Host "🔨 Limpiando proyecto..." -ForegroundColor Cyan
Set-Location $projectPath
dotnet clean -c Release

Write-Host "`n📦 Generando APK firmado..." -ForegroundColor Cyan
$password = Read-Host "Ingresa la contraseña del keystore" -AsSecureString
$passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

dotnet publish -f net9.0-android -c Release /p:AndroidSigningPassword=$passwordPlain

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ APK generado exitosamente" -ForegroundColor Green
    
    if (Test-Path $outputApk) {
        Write-Host "`n📋 Copiando APK a wwwroot..." -ForegroundColor Cyan
        Copy-Item $outputApk "..\$destinationWeb" -Force
        
        $fileSize = (Get-Item "..\$destinationWeb").Length / 1MB
        Write-Host "✅ APK copiado: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Green
        Write-Host "`n📍 Ubicación: $destinationWeb" -ForegroundColor Yellow
    } else {
        Write-Host "❌ No se encontró el APK en: $outputApk" -ForegroundColor Red
    }
} else {
    Write-Host "`n❌ Error al generar APK" -ForegroundColor Red
    exit 1
}

Set-Location ..
Write-Host "`n🎉 Proceso completado" -ForegroundColor Green