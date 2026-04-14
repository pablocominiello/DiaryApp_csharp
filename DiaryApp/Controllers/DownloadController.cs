using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.Controllers
{
    public class DownloadController : Controller
    {
        public IActionResult Index()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "DiaryApp.apk");
            
            ViewBag.AppVersion = "1.0.0";
            ViewBag.ApkSize = 0L;

            if (System.IO.File.Exists(apkPath))
            {
                var fileInfo = new FileInfo(apkPath);
                ViewBag.ApkSize = fileInfo.Length / 1024 / 1024; // MB
            }

            return View();
        }
    }
}