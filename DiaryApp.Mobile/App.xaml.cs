using DiaryApp.Mobile.Services;
using DiaryApp.Mobile.Views;

namespace DiaryApp.Mobile;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
	{
		InitializeComponent();
		_authService = authService;

		// ✅ Capturar excepciones no manejadas
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		MainPage = new AppShell();
	}

	private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		var exception = e.ExceptionObject as Exception;
		System.Diagnostics.Debug.WriteLine($"❌ UNHANDLED EXCEPTION: {exception?.Message}");
		System.Diagnostics.Debug.WriteLine($"StackTrace: {exception?.StackTrace}");
		
		// Mostrar alerta al usuario
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			if (MainPage != null)
			{
				await MainPage.DisplayAlert(
					"Error crítico",
					$"La aplicación encontró un error:\n\n{exception?.Message}\n\nLa app se cerrará.",
					"OK");
			}
		});
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"❌ UNOBSERVED TASK EXCEPTION: {e.Exception?.Message}");
		System.Diagnostics.Debug.WriteLine($"StackTrace: {e.Exception?.StackTrace}");
		e.SetObserved(); // Evita que la app se cierre
	}

    protected override async void OnStart()
    {
        base.OnStart();
        
        System.Diagnostics.Debug.WriteLine("===== APP STARTED =====");
        System.Diagnostics.Debug.WriteLine($"Device: {DeviceInfo.Current.Name}");
        System.Diagnostics.Debug.WriteLine($"Platform: {DeviceInfo.Current.Platform}");
        System.Diagnostics.Debug.WriteLine($"Version: {DeviceInfo.Current.VersionString}");
        
        try
        {
#if DEBUG
            // ✅ NUEVO: No hacer logout si hay un share intent activo
            if (!ShareIntentHandler.HasSharedIntent())
            {
                await _authService.LogoutAsync();
                System.Diagnostics.Debug.WriteLine("DEBUG: Sesión limpiada automáticamente");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Sesión MANTENIDA por share intent");
            }
#endif
            
            await CheckAuthenticationAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ OnStart Exception: {ex}");
            await MainPage.DisplayAlert("Error de inicio", ex.Message, "OK");
        }
    }

    private async Task CheckAuthenticationAsync()
    {
        try
        {
            // Esperar un momento para asegurar que Shell.Current esté disponible
            await Task.Delay(100);

            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            System.Diagnostics.Debug.WriteLine($"IsAuthenticated: {isAuthenticated}");
            
            // ✅ NUEVO: Si hay un share intent, ir a payments directamente
            if (ShareIntentHandler.HasSharedIntent())
            {
                System.Diagnostics.Debug.WriteLine("🚀 Share intent detectado - navegando a payments");
                await Shell.Current.GoToAsync("///payments");
                return; // MainActivity se encargará de la navegación al detalle
            }
            
            if (isAuthenticated)
            {
                // Usuario ya tiene sesión activa
                System.Diagnostics.Debug.WriteLine("Navegando a /persons");
                await Shell.Current.GoToAsync("///persons");
            }
            else
            {
                // Usuario no autenticado, ir a login
                System.Diagnostics.Debug.WriteLine("Navegando a /login");
                await Shell.Current.GoToAsync("///login");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking auth: {ex.Message}");
            
            // Intentar navegar a login como fallback
            try
            {
                await Shell.Current.GoToAsync("///login");
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
            }
        }
    }
}