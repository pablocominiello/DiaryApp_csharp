using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace DiaryApp.Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    LaunchMode = LaunchMode.SingleTop, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    Exported = true)] // ✅ IMPORTANTE: Exported=true para que otras apps puedan invocarla
// ✅ INTENT FILTER para recibir imágenes compartidas
[IntentFilter(
    new[] { Intent.ActionSend },
    Categories = new[] { Intent.CategoryDefault },
    DataMimeType = "image/*",
    Label = "9Julio")] // ✅ Nombre que aparecerá en el menú de compartir
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // ✅ Procesar intent compartido si existe
        HandleSharedIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        if (intent != null)
        {
            Intent = intent;
            HandleSharedIntent(intent);
        }
    }

    private void HandleSharedIntent(Intent? intent)
    {
        if (intent?.Action == Intent.ActionSend && intent.Type?.StartsWith("image/") == true)
        {
            System.Diagnostics.Debug.WriteLine("📸 Imagen compartida detectada");
            
            // Obtener la URI de la imagen compartida
            var imageUri = intent.GetParcelableExtra(Intent.ExtraStream) as Android.Net.Uri;
            
            if (imageUri != null)
            {
                System.Diagnostics.Debug.WriteLine($"📄 URI: {imageUri}");
                
                // Guardar la URI globalmente para procesarla en la app
                DiaryApp.Mobile.Services.SharedImageHandler.SetSharedImage(imageUri);
                
                // Navegar a la página de creación de pago
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(500); // Esperar a que la app esté lista
                    
                    // Verificar que Shell.Current esté disponible
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("//payments/new");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Shell.Current no está disponible aún");
                    }
                });
            }
        }
    }
}
