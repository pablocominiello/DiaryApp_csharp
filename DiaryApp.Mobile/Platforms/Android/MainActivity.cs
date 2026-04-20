using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace DiaryApp.Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    LaunchMode = LaunchMode.SingleTask, // ✅ CAMBIADO a SingleTask para evitar duplicados
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    Exported = true)]
// ✅ INTENT FILTER para recibir imágenes compartidas
[IntentFilter(
    new[] { Intent.ActionSend },
    Categories = new[] { Intent.CategoryDefault },
    DataMimeType = "image/*",
    Label = "9Julio")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        System.Diagnostics.Debug.WriteLine("📱 MainActivity.OnCreate called");
        
        // ✅ Procesar intent compartido si existe
        HandleSharedIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        
        System.Diagnostics.Debug.WriteLine("📱 MainActivity.OnNewIntent called");
        
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
                
                // ✅ MARCAR que tenemos un share intent activo
                DiaryApp.Mobile.Services.ShareIntentHandler.SetSharedIntentReceived(true);
                
                // Guardar la URI globalmente para procesarla en la app
                DiaryApp.Mobile.Services.SharedImageHandler.SetSharedImage(imageUri);
                
                // ✅ Navegar después de que la app esté completamente inicializada
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Esperar más tiempo para asegurar que Shell esté listo
                    await Task.Delay(1500);
                    
                    if (Shell.Current != null)
                    {
                        System.Diagnostics.Debug.WriteLine("🚀 Navegando a payments/new");
                        
                        try
                        {
                            // Navegar directamente a la página de nuevo pago
                            await Shell.Current.GoToAsync("///payments");
                            await Task.Delay(300);
                            await Shell.Current.GoToAsync("PaymentDetailPage");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Error navegando: {ex.Message}");
                        }
                        finally
                        {
                            // Limpiar el flag después de navegar
                            DiaryApp.Mobile.Services.ShareIntentHandler.Clear();
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Shell.Current no está disponible");
                    }
                });
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"📱 Intent normal - Action: {intent?.Action}, Type: {intent?.Type}");
        }
    }
}
