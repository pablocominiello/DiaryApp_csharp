# Script para generar íconos PWA desde la imagen existente
$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🎨 Generador de Íconos PWA - 9 de Julio             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Rutas
$sourceImage = "DiaryApp\wwwroot\images\9Julio\9 de julio azul.png"
$outputDir = "DiaryApp\wwwroot\images\9Julio"
$icon192 = Join-Path $outputDir "icon-192.png"
$icon512 = Join-Path $outputDir "icon-512.png"

# Verificar que existe la imagen fuente
if (-Not (Test-Path $sourceImage)) {
    Write-Host "❌ ERROR: No se encontró la imagen fuente:" -ForegroundColor Red
    Write-Host "   $sourceImage" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Imagen fuente encontrada: $sourceImage" -ForegroundColor Green
Write-Host ""

# Cargar el ensamblado de System.Drawing
Add-Type -AssemblyName System.Drawing

try {
    Write-Host "📸 Cargando imagen original..." -ForegroundColor Cyan
    $originalImage = [System.Drawing.Image]::FromFile((Resolve-Path $sourceImage))
    
    Write-Host "   Dimensiones originales: $($originalImage.Width)x$($originalImage.Height)" -ForegroundColor Gray
    Write-Host ""
    
    # Función para redimensionar y guardar imagen
    function Resize-Image {
        param(
            [System.Drawing.Image]$sourceImage,
            [int]$targetSize,
            [string]$outputPath
        )
        
        Write-Host "🔄 Generando ícono ${targetSize}x${targetSize}..." -ForegroundColor Yellow
        
        # Crear nueva imagen con fondo transparente
        $newImage = New-Object System.Drawing.Bitmap($targetSize, $targetSize)
        $graphics = [System.Drawing.Graphics]::FromImage($newImage)
        
        # Configurar calidad de renderizado
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        
        # Limpiar con fondo transparente (o blanco si prefieres)
        $graphics.Clear([System.Drawing.Color]::Transparent)
        # Si prefieres fondo blanco, usa: $graphics.Clear([System.Drawing.Color]::White)
        
        # Calcular dimensiones manteniendo aspecto
        $ratioX = $targetSize / $sourceImage.Width
        $ratioY = $targetSize / $sourceImage.Height
        $ratio = [Math]::Min($ratioX, $ratioY)
        
        $newWidth = [int]($sourceImage.Width * $ratio)
        $newHeight = [int]($sourceImage.Height * $ratio)
        
        # Centrar la imagen
        $posX = [int](($targetSize - $newWidth) / 2)
        $posY = [int](($targetSize - $newHeight) / 2)
        
        # Dibujar imagen redimensionada
        $graphics.DrawImage(
            $sourceImage, 
            $posX, $posY, 
            $newWidth, $newHeight
        )
        
        # Guardar como PNG
        $newImage.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        
        # Limpiar recursos
        $graphics.Dispose()
        $newImage.Dispose()
        
        $fileInfo = Get-Item $outputPath
        $fileSizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
        Write-Host "   ✅ Guardado: $outputPath ($fileSizeKB KB)" -ForegroundColor Green
    }
    
    # Generar ícono 192x192
    Resize-Image -sourceImage $originalImage -targetSize 192 -outputPath $icon192
    
    # Generar ícono 512x512
    Resize-Image -sourceImage $originalImage -targetSize 512 -outputPath $icon512
    
    # Limpiar recursos
    $originalImage.Dispose()
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  ✅ ÍCONOS PWA GENERADOS EXITOSAMENTE                 ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📋 Archivos generados:" -ForegroundColor Cyan
    Write-Host "   📱 icon-192.png (192x192) - Para pantallas normales" -ForegroundColor Gray
    Write-Host "   📱 icon-512.png (512x512) - Para pantallas de alta resolución" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "🎯 Próximos pasos:" -ForegroundColor Cyan
    Write-Host "   1️⃣  Verifica que los archivos manifest.json y sw.js estén creados" -ForegroundColor Yellow
    Write-Host "   2️⃣  Actualiza _Layout.cshtml con las referencias PWA" -ForegroundColor Yellow
    Write-Host "   3️⃣  Despliega los cambios en Azure" -ForegroundColor Yellow
    Write-Host "   4️⃣  Prueba en Chrome móvil: Menú > Agregar a pantalla de inicio" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "💡 Consejo: Los íconos tienen fondo transparente. Si prefieres" -ForegroundColor Cyan
    Write-Host "   fondo blanco, edita el script y cambia 'Transparent' por 'White'" -ForegroundColor Cyan
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "❌ ERROR al generar íconos:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Verifica que tengas .NET Framework instalado (viene con Windows)" -ForegroundColor Cyan
    exit 1
}

Write-Host "🎉 Proceso completado" -ForegroundColor Green