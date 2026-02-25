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

		// ✅ CRITICAL: Configurar SQLite Database (faltaba esto!)
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diaryapp.db3");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Filename={dbPath}"));

		// ✅ Configurar Azure Blob Storage connection string
		var azureBlobConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") 
			?? "DefaultEndpointsProtocol=https;AccountName=devaccount;AccountKey=devkey;EndpointSuffix=core.windows.net";
		
		// ✅ Registrar BlobStorageService con factory
		builder.Services.AddSingleton<IBlobStorageService>(sp =>
		{
			return new BlobStorageService(azureBlobConnectionString);
		});

		// ✅ Registrar HttpClient y ApiService apuntando a Azure
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			// Apuntar a tu API en Azure
			client.BaseAddress = new Uri("https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/");
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// ✅ Registrar IDatabaseService (ahora AppDbContext está disponible)
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
