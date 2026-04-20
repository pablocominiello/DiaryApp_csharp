using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

public partial class DiagnosticsViewModel : BaseViewModel
{
    // ✅ SOLO estas propiedades
    [ObservableProperty]
    private string apiUrl = string.Empty;

    [ObservableProperty]
    private string apiStatus = string.Empty;

    [ObservableProperty]
    private string appDataDirectory = string.Empty;

    [ObservableProperty]
    private int personCount;

    [ObservableProperty]
    private int diaryEntryCount;

    [ObservableProperty]
    private int paymentCount;

    [ObservableProperty]
    private string connectionMode = string.Empty;

    // ✅ NUEVO: Propiedades para versión
    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string appBuild = string.Empty;

    private readonly IApiService _apiService;

    public DiagnosticsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Diagnóstico";
        
#if DEBUG
        ApiUrl = "https://10.0.2.2:7001/api";
        ConnectionMode = "🔵 DEBUG - Servidor Local";
#else
        ApiUrl = "https://dev-diaryapp-c2cuanhkf2f6axee.canadacentral-01.azurewebsites.net/api";
        ConnectionMode = "🟢 RELEASE - Azure Cloud";
#endif

        AppDataDirectory = FileSystem.AppDataDirectory;
        
        // ✅ NUEVO: Obtener versión de la app
        LoadAppVersion();
        
        LoadDatabaseInfo();
    }

    // ✅ NUEVO: Método para cargar versión
    private void LoadAppVersion()
    {
        try
        {
            AppVersion = AppInfo.Current.VersionString;
            AppBuild = AppInfo.Current.BuildString;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error getting app version: {ex.Message}");
            AppVersion = "N/A";
            AppBuild = "N/A";
        }
    }

    [RelayCommand]
    private async Task LoadDatabaseInfo()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ApiStatus = "🔄 Conectando...";

            var persons = await _apiService.GetPersonsAsync();
            PersonCount = persons?.Count ?? 0;

            var entries = await _apiService.GetDiaryEntriesAsync();
            DiaryEntryCount = entries?.Count ?? 0;

            var payments = await _apiService.GetPaymentsAsync();
            PaymentCount = payments?.Count ?? 0;

            ApiStatus = "✅ Conectado correctamente";
        }
        catch (HttpRequestException ex)
        {
            ApiStatus = $"❌ Error de conexión: {ex.Message}";
            await Shell.Current.DisplayAlert("Error de Conexión", 
                $"No se pudo conectar al servidor:\n{ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            ApiStatus = $"❌ Error: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", 
                $"Error al cargar información:\n{ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CopyApiUrlAsync()
    {
        await Clipboard.SetTextAsync(ApiUrl);
        await Shell.Current.DisplayAlert("Copiado", 
            "URL del API copiada al portapapeles", "OK");
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        await LoadDatabaseInfo();
    }
}