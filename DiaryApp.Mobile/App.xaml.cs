using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile;

public partial class App : Application
{
    public App(IDatabaseService databaseService)
    {
        InitializeComponent();

                    // Inicializar la base de datos en un hilo en segundo plano
        Task.Run(async () =>
        {
            await databaseService.InitializeDatabaseAsync();
        });

        MainPage = new AppShell();
    }
}