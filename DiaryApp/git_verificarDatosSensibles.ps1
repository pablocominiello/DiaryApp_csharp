Write-Host "`n=== Verificando archivos sensibles ===" -ForegroundColor Cyan

Write-Host "`n1. DiaryApp/appsettings.Development.json" -ForegroundColor Yellow
if (Test-Path "DiaryApp/appsettings.Development.json") {
    Get-Content "DiaryApp/appsettings.Development.json" | Select-Object -First 20
} else {
    Write-Host "  ✅ No existe" -ForegroundColor Green
}

Write-Host "`n2. appsettings.Development.json (raíz)" -ForegroundColor Yellow
if (Test-Path "appsettings.Development.json") {
    Get-Content "appsettings.Development.json" | Select-Object -First 20
} else {
    Write-Host "  ✅ No existe" -ForegroundColor Green
}

Write-Host "`n3. k8s-minikube/secrets.yaml" -ForegroundColor Yellow
if (Test-Path "k8s-minikube/secrets.yaml") {
    Get-Content "k8s-minikube/secrets.yaml" | Select-Object -First 20
} else {
    Write-Host "  ✅ No existe" -ForegroundColor Green
}

Write-Host "`n4. k8s/secrets.yaml" -ForegroundColor Yellow
if (Test-Path "k8s/secrets.yaml") {
    Get-Content "k8s/secrets.yaml" | Select-Object -First 20
} else {
    Write-Host "  ✅ No existe" -ForegroundColor Green
}