using DiaryApp.Mobile.Views;

namespace DiaryApp.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Registrar rutas para navegación
		Routing.RegisterRoute(nameof(PersonDetailPage), typeof(PersonDetailPage));
		Routing.RegisterRoute(nameof(DiaryEntryDetailPage), typeof(DiaryEntryDetailPage));
		Routing.RegisterRoute(nameof(PaymentDetailPage), typeof(PaymentDetailPage));
		Routing.RegisterRoute(nameof(DiagnosticsPage), typeof(DiagnosticsPage)); // NUEVO
	}
}
