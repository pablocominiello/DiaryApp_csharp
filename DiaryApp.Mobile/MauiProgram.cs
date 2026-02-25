using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using DiaryApp.Mobile.Data;
using DiaryApp.Mobile.Services;
using DiaryApp.Mobile.ViewModels;
using DiaryApp.Mobile.Views;

namespace DiaryApp.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Configurar la base de datos SQLite
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diaryapp.db3");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Filename={dbPath}"));

		// Registrar servicios
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
		builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

		// Registrar ViewModels
		builder.Services.AddTransient<PersonsViewModel>();
		builder.Services.AddTransient<PersonDetailViewModel>();
		builder.Services.AddTransient<DiaryEntriesViewModel>();
		builder.Services.AddTransient<DiaryEntryDetailViewModel>();
		builder.Services.AddTransient<PaymentsViewModel>();
		builder.Services.AddTransient<PaymentDetailViewModel>();
		builder.Services.AddTransient<DiagnosticsViewModel>(); // NUEVO

		// Registrar Views
		builder.Services.AddTransient<PersonsPage>();
		builder.Services.AddTransient<PersonDetailPage>();
		builder.Services.AddTransient<DiaryEntriesPage>();
		builder.Services.AddTransient<DiaryEntryDetailPage>();
		builder.Services.AddTransient<PaymentsPage>();
		builder.Services.AddTransient<PaymentDetailPage>();
		builder.Services.AddTransient<DiagnosticsPage>(); // NUEVO

		return builder.Build();
	}
}
