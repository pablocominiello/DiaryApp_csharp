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

		// Configurar la URL base del API - DESARROLLO
		const string apiBaseUrl = "https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api/";

		// Registrar HttpClient para AuthService
		builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Blob Storage
		builder.Services.AddSingleton<IBlobStorageService>(sp =>
		{
			var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
			
			if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("devaccount"))
			{
				System.Diagnostics.Debug.WriteLine("Azure Blob Storage no configurado - usando implementacion local");
				return new LocalBlobStorageService();
			}
			
			return new BlobStorageService(connectionString);
		});

		// ApiService (necesita IAuthService inyectado)
		builder.Services.AddHttpClient<IApiService, ApiService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<PersonsViewModel>();
		builder.Services.AddTransient<PersonDetailViewModel>();
		builder.Services.AddTransient<DiaryEntriesViewModel>();
		builder.Services.AddTransient<DiaryEntryDetailViewModel>();
		builder.Services.AddTransient<PaymentsViewModel>();
		builder.Services.AddTransient<PaymentDetailViewModel>();
		builder.Services.AddTransient<DiagnosticsViewModel>();

		// Views
		builder.Services.AddTransient<LoginPage>();
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
