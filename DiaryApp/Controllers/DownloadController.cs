using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> _logger;

        public DownloadController(ILogger<DownloadController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "DiaryApp.apk");
            
            ViewBag.AppVersion = "1.0.0";
            ViewBag.ApkSize = 0L;
            ViewBag.ApkSizeFormatted = "No disponible";

            if (System.IO.File.Exists(apkPath))
            {
                var fileInfo = new FileInfo(apkPath);
                
                // Convertir a MB con decimales
                double sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                long sizeRounded = (long)Math.Ceiling(sizeInMB);
                string sizeFormatted = $"{sizeInMB:F2} MB";
                
                ViewBag.ApkSize = sizeRounded;
                ViewBag.ApkSizeFormatted = sizeFormatted;
                
                // ✅ FIX: Usar variables tipadas en lugar de ViewBag
                _logger.LogInformation("APK encontrado: {Bytes} bytes ({Size})", fileInfo.Length, sizeFormatted);
            }
            else
            {
                _logger.LogWarning("APK no encontrado en: {Path}", apkPath);
            }

            return View();
        }
    }
}