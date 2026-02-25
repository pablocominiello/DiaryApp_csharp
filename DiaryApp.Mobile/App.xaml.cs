using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile;

public partial class App : Application
{
	private readonly IDatabaseService _databaseService;

	public App(IDatabaseService databaseService)
	{
		InitializeComponent();
		_databaseService = databaseService;
		
		MainPage = new AppShell();
		
		// Inicializar BD de forma asíncrona
		InitializeAppAsync();
	}

	private async void InitializeAppAsync()
	{
		try
		{
			await _databaseService.InitializeDatabaseAsync();
		}
		catch (Exception ex)
		{
			// Mostrar error al usuario
			await MainPage.DisplayAlert(
				"Error de Inicialización",
				$"No se pudo inicializar la base de datos: {ex.Message}",
				"OK");
		}
	}
}