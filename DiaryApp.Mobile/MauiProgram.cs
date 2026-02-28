using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using DiaryApp.Mobile.Data;
using DiaryApp.Mobile.Services;
using DiaryApp.Mobile.ViewModels;
using DiaryApp.Mobile.Views;
using Microsoft.Extensions.Configuration;

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

		// ✅ CRITICAL: Configurar SQLite Database
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diaryapp.db3");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Filename={dbPath}"));

		// ✅ CORREGIDO: Registrar BlobStorageService sin requerir conexión válida
		// Si no tienes Azure Blob Storage configurado, usa una implementación dummy
		builder.Services.AddSingleton<IBlobStorageService>(sp =>
		{
			var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
			
			// Si no hay connection string válida, retornar una implementación que no requiera Azure
			if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("devaccount"))
			{
				System.Diagnostics.Debug.WriteLine("⚠️ Azure Blob Storage no configurado - usando implementación local");
				return new LocalBlobStorageService(); // Implementación que guarda archivos localmente
			}
			
			return new BlobStorageService(connectionString);
		});

		// ✅ Registrar HttpClient y ApiService apuntando a Azure
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			// Apuntar a tu API en Azure
			client.BaseAddress = new Uri("https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/");
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// ✅ Registrar IDatabaseService
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

		// Registrar ViewModels
		builder.Services.AddTransient<PersonsViewModel>();
		builder.Services.AddTransient<PersonDetailViewModel>();
		builder.Services.AddTransient<DiaryEntriesViewModel>();
		builder.Services.AddTransient<DiaryEntryDetailViewModel>();
		builder.Services.AddTransient<PaymentsViewModel>();
		builder.Services.AddTransient<PaymentDetailViewModel>();
		builder.Services.AddTransient<DiagnosticsViewModel>();

		// Registrar Views
		builder.Services.AddTransient<PersonsPage>();
		builder.Services.AddTransient<PersonDetailPage>();
		builder.Services.AddTransient<DiaryEntriesPage>();
		builder.Services.AddTransient<DiaryEntryDetailPage>();
		builder.Services.AddTransient<PaymentsPage>();
		builder.Services.AddTransient<PaymentDetailPage>();
		builder.Services.AddTransient<DiagnosticsPage>();

		return builder.Build();
	}
}
