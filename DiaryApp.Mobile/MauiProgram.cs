using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
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

		// ✅ Registrar BlobStorageService (implementación local para desarrollo)
		builder.Services.AddSingleton<IBlobStorageService>(sp =>
		{
			var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
			
			// Si no hay connection string válida, usar implementación local
			if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("devaccount"))
			{
				System.Diagnostics.Debug.WriteLine("⚠️ Azure Blob Storage no configurado - usando implementación local");
				return new LocalBlobStorageService();
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
