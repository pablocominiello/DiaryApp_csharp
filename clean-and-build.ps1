# Script de limpieza completa
$solutionPath = "C:\Fuentes\VS\DiaryApp_csharp\DiaryApp_csharp"

Write-Host "🧹 Limpiando solución DiaryApp..." -ForegroundColor Cyan

# 1. Eliminar carpeta Data de DiaryApp.Api si existe
$apiDataPath = Join-Path $solutionPath "DiaryApp.Api\Data"
if (Test-Path $apiDataPath) {
    Write-Host "❌ Eliminando DiaryApp.Api\Data..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $apiDataPath
    Write-Host "✅ DiaryApp.Api\Data eliminada" -ForegroundColor Green
}

# 2. Eliminar todas las carpetas bin y obj
Write-Host "`n🗑️ Eliminando carpetas bin y obj..." -ForegroundColor Yellow
Get-ChildItem -Path $solutionPath -Include bin,obj -Recurse -Force | Remove-Item -Force -Recurse
Write-Host "✅ Carpetas bin/obj eliminadas" -ForegroundColor Green

# 3. Compilar en orden
Write-Host "`n🔨 Compilando proyectos..." -ForegroundColor Cyan

Set-Location "$solutionPath\DiaryApp.Shared"
Write-Host "`n1️⃣ Compilando DiaryApp.Shared..." -ForegroundColor Yellow
dotnet build --no-incremental
if ($LASTEXITCODE -eq 0) { Write-Host "✅ OK" -ForegroundColor Green } else { Write-Host "❌ ERROR" -ForegroundColor Red; exit 1 }

Set-Location "$solutionPath\DiaryApp.Core"
Write-Host "`n2️⃣ Compilando DiaryApp.Core..." -ForegroundColor Yellow
dotnet build --no-incremental
if ($LASTEXITCODE -eq 0) { Write-Host "✅ OK" -ForegroundColor Green } else { Write-Host "❌ ERROR" -ForegroundColor Red; exit 1 }

Set-Location "$solutionPath\DiaryApp.Api"
Write-Host "`n3️⃣ Compilando DiaryApp.Api..." -ForegroundColor Yellow
dotnet build --no-incremental
if ($LASTEXITCODE -eq 0) { Write-Host "✅ OK" -ForegroundColor Green } else { Write-Host "❌ ERROR" -ForegroundColor Red; exit 1 }

Set-Location "$solutionPath\DiaryApp"
Write-Host "`n4️⃣ Compilando DiaryApp (Web)..." -ForegroundColor Yellow
dotnet build --no-incremental
if ($LASTEXITCODE -eq 0) { Write-Host "✅ OK" -ForegroundColor Green } else { Write-Host "❌ ERROR" -ForegroundColor Red; exit 1 }

Set-Location $solutionPath
Write-Host "`n🎉 ¡Compilación completada exitosamente!" -ForegroundColor Green