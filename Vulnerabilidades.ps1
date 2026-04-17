# ===== SCRIPT COMPLETO DE LIMPIEZA =====
Write-Host "`n🔒 INICIANDO LIMPIEZA DE SEGURIDAD" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# 1. Remover archivos sensibles
Write-Host "`n📝 Paso 1: Removiendo archivos sensibles..." -ForegroundColor Yellow
git rm --cached "DiaryApp/appsettings.Development.json" 2>$null
git rm --cached "appsettings.Development.json" 2>$null
git rm --cached "k8s-minikube/secrets.yaml" 2>$null
git rm --cached "k8s/secrets.yaml" 2>$null

# 2. Agregar .gitignore
Write-Host "`n📝 Paso 2: Actualizando .gitignore..." -ForegroundColor Yellow
git add .gitignore

# 3. Agregar archivos modificados
git add DiaryApp/appsettings.json
git add DiaryApp.Mobile/MauiProgram.cs

# 4. Ver estado
Write-Host "`n📊 Estado actual de Git:" -ForegroundColor Cyan
git status

# 5. Esperar confirmación
Write-Host "`n⚠️ Revisa los cambios arriba. ¿Hacer commit? (S/N)" -ForegroundColor Yellow
$confirm = Read-Host "Confirmar"

if ($confirm -eq 'S' -or $confirm -eq 's') {
    git commit -m "Security: Remove sensitive files and update configuration

- Remove files with exposed passwords from Git tracking
- Update .gitignore with comprehensive security patterns
- Clean appsettings.json placeholder
- Update MauiProgram.cs with better API URL configuration

Files removed from tracking (still exist locally):
- DiaryApp/appsettings.Development.json
- appsettings.Development.json (contained Azure SQL password)
- k8s-minikube/secrets.yaml
- k8s/secrets.yaml

CRITICAL: Passwords need to be rotated immediately"
    
    Write-Host "`n✅ Commit realizado exitosamente" -ForegroundColor Green
    Write-Host "`n¿Hacer push a GitHub? (S/N)" -ForegroundColor Yellow
    $pushConfirm = Read-Host "Confirmar push"
    
    if ($pushConfirm -eq 'S' -or $pushConfirm -eq 's') {
        git push origin master
        Write-Host "`n✅ Cambios subidos a GitHub" -ForegroundColor Green
        Write-Host "`n⚠️ SIGUIENTE PASO CRÍTICO: Cambiar contraseñas expuestas" -ForegroundColor Red
    } else {
        Write-Host "`n⏸️ Push cancelado. Ejecuta 'git push origin master' cuando estés listo" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n❌ Commit cancelado" -ForegroundColor Red
}

Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "🔒 PROCESO COMPLETADO" -ForegroundColor Cyan