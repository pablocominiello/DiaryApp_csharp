using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile;

public partial class App : Application
{
	public App(IDatabaseService databaseService)
	{
		InitializeComponent();

		// Inicializar la base de datos
		Task.Run(async () =>
		{
			await databaseService.InitializeDatabaseAsync();
		});
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}