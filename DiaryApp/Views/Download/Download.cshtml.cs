using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiaryApp.Pages
{
    public class DownloadModel : PageModel
    {
        public string AppVersion { get; set; } = "1.0.0";
        public long ApkSize { get; set; }

        public void OnGet()
        {
            var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "x9JulioApp.x9JulioApp.apk");
            if (System.IO.File.Exists(apkPath))
            {
                var fileInfo = new FileInfo(apkPath);
                ApkSize = fileInfo.Length / 1024 / 1024; // MB
            }
        }
    }
}