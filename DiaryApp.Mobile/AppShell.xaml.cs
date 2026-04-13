using DiaryApp.Mobile.Views;

namespace DiaryApp.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Registrar rutas para navegación
		try
		{
			Routing.RegisterRoute(nameof(PersonDetailPage), typeof(PersonDetailPage));
			// COMENTADO: DiaryEntries eliminado del menú
			// Routing.RegisterRoute(nameof(DiaryEntryDetailPage), typeof(DiaryEntryDetailPage));
			Routing.RegisterRoute(nameof(PaymentDetailPage), typeof(PaymentDetailPage));
			Routing.RegisterRoute(nameof(DiagnosticsPage), typeof(DiagnosticsPage));
			
			System.Diagnostics.Debug.WriteLine("All routes registered successfully");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Route registration error: {ex.Message}");
		}
	}
}
