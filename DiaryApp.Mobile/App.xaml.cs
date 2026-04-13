using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // Manejar excepciones globales
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"UNHANDLED EXCEPTION: {exception?.Message}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"UNOBSERVED TASK EXCEPTION: {e.Exception?.Message}");
            e.SetObserved();
        };

        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        // En DEBUG, limpiar sesión cada vez que inicia (para pruebas)
#if DEBUG
        await _authService.LogoutAsync();
        System.Diagnostics.Debug.WriteLine("DEBUG: Sesión limpiada automáticamente");
#endif
        
        // Verificar autenticación cuando la app inicia
        await CheckAuthenticationAsync();
    }

    private async Task CheckAuthenticationAsync()
    {
        try
        {
            // Esperar un momento para asegurar que Shell.Current esté disponible
            await Task.Delay(100);

            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            System.Diagnostics.Debug.WriteLine($"IsAuthenticated: {isAuthenticated}");
            
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