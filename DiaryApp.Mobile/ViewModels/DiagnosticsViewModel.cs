using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;
using System.Net.Http;

namespace DiaryApp.Mobile.ViewModels;

public partial class DiagnosticsViewModel : BaseViewModel
{
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

    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string appBuild = string.Empty;

    private readonly IApiService _apiService;
    private readonly HttpClient _httpClient;

    public DiagnosticsViewModel(IApiService apiService, HttpClient httpClient)
    {
        _apiService = apiService;
        _httpClient = httpClient;
        Title = "Diagnóstico";
        
        // ✅ NUEVO: Obtener la URL real del HttpClient inyectado
        ApiUrl = _httpClient.BaseAddress?.ToString() ?? "No configurada";
        
        // ✅ Determinar el modo de conexión basado en la URL real
        if (ApiUrl.Contains("localhost") || ApiUrl.Contains("10.0.2.2") || ApiUrl.Contains("127.0.0.1"))
        {
            ConnectionMode = "🔵 DEBUG - Servidor Local";
        }
        else if (ApiUrl.Contains("azure") || ApiUrl.Contains("canadacentral"))
        {
            ConnectionMode = "🟢 RELEASE - Azure Cloud";
        }
        else
        {
            ConnectionMode = "⚪ Desconocido";
        }

        AppDataDirectory = FileSystem.AppDataDirectory;
        
        LoadAppVersion();
        LoadDatabaseInfo();
    }

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