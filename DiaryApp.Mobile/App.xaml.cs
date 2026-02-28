using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile;

public partial class App : Application
{
    public App(IDatabaseService databaseService)
    {
        InitializeComponent();

        // ✅ Agregar manejo global de excepciones
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"❌ UNHANDLED EXCEPTION: {exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ STACK TRACE: {exception?.StackTrace}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"❌ UNOBSERVED TASK EXCEPTION: {e.Exception?.Message}");
            e.SetObserved();
        };

        // Inicializar la base de datos en un hilo en segundo plano
        Task.Run(async () =>
        {
            try
            {
                await databaseService.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DATABASE INIT ERROR: {ex.Message}");
            }
        });

        MainPage = new AppShell();
    }
}