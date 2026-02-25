using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

public partial class DiagnosticsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string databasePath = string.Empty;

    [ObservableProperty]
    private bool databaseExists;

    [ObservableProperty]
    private string databaseSize = string.Empty;

    [ObservableProperty]
    private string appDataDirectory = string.Empty;

    [ObservableProperty]
    private int personCount;

    [ObservableProperty]
    private int diaryEntryCount;

    [ObservableProperty]
    private int paymentCount;

    private readonly IDatabaseService _databaseService;

    public DiagnosticsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Diagnóstico";
        LoadDatabaseInfo();
    }

    [RelayCommand]
    private async Task LoadDatabaseInfo()
    {
        try
        {
            IsBusy = true;

            // Información de rutas
            DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "diaryapp.db3");
            AppDataDirectory = FileSystem.AppDataDirectory;
            DatabaseExists = File.Exists(DatabasePath);

            if (DatabaseExists)
            {
                var fileInfo = new FileInfo(DatabasePath);
                DatabaseSize = $"{fileInfo.Length / 1024.0:F2} KB";
            }
            else
            {
                DatabaseSize = "N/A";
            }

            // Contar registros
            var persons = await _databaseService.GetPersonsAsync();
            PersonCount = persons.Count;

            var entries = await _databaseService.GetDiaryEntriesAsync();
            DiaryEntryCount = entries.Count;

            var payments = await _databaseService.GetPaymentsAsync();
            PaymentCount = payments.Count;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error al cargar información: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CopyPathAsync()
    {
        await Clipboard.SetTextAsync(DatabasePath);
        await Shell.Current.DisplayAlert("Copiado", "Ruta copiada al portapapeles", "OK");
    }

    [RelayCommand]
    private async Task CopyDirectoryAsync()
    {
        await Clipboard.SetTextAsync(AppDataDirectory);
        await Shell.Current.DisplayAlert("Copiado", "Directorio copiado al portapapeles", "OK");
    }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri($"file://{FileSystem.AppDataDirectory}"));
        }
        catch
        {
            await Shell.Current.DisplayAlert("Info", 
                "No se puede abrir la carpeta automáticamente. Usa la ruta copiada.", "OK");
        }
    }
}