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
        
        // Verificar autenticación cuando la app inicia (Shell ya está disponible aquí)
        await CheckAuthenticationAsync();
    }

    private async Task CheckAuthenticationAsync()
    {
        try
        {
            // Esperar un momento para asegurar que Shell.Current esté disponible
            await Task.Delay(100);

            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                // Usuario ya tiene sesión activa
                await Shell.Current.GoToAsync("///persons");
            }
            else
            {
                // Usuario no autenticado, ir a login
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
            catch
            {
                // Si Shell aún no está disponible, no hacer nada
                System.Diagnostics.Debug.WriteLine("Shell.Current no disponible todavía");
            }
        }
    }
}