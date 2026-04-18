using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Shared.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
[QueryProperty(nameof(PersonId), nameof(PersonId))]
[QueryProperty(nameof(PersonName), nameof(PersonName))]
public partial class PaymentDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IBlobStorageService _blobStorageService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int personId;

    [ObservableProperty]
    private string? personName;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private string? comentary;

    [ObservableProperty]
    private int ano = DateTime.Now.Year;

    [ObservableProperty]
    private int mes = DateTime.Now.Month;

    [ObservableProperty]
    private DateTime fecha = DateTime.Now;

    [ObservableProperty]
    private string? comprobanteUrl;

    [ObservableProperty]
    private ImageSource? comprobantePreview;

    [ObservableProperty]
    private string? comprobanteFileName;

    [ObservableProperty]
    private bool hasComprobante;

    private byte[]? _comprobanteData;

    public PaymentDetailViewModel(IApiService apiService, IBlobStorageService blobStorageService)
    {
        _apiService = apiService;
        _blobStorageService = blobStorageService;
        Title = "Nuevo Pago";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadPaymentAsync(value);
        }
    }

    partial void OnPersonIdChanged(int value)
    {
        if (value > 0 && string.IsNullOrEmpty(PersonName))
        {
            _ = LoadPersonNameAsync(value);
        }
    }

    private async Task LoadPersonNameAsync(int personId)
    {
        try
        {
            var person = await _apiService.GetPersonAsync(personId);
            if (person != null)
            {
                PersonName = person.Nombre;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading person: {ex.Message}");
        }
    }

    private async Task LoadPaymentAsync(int paymentId)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var payment = await _apiService.GetPaymentAsync(paymentId);
            if (payment != null)
            {
                PersonId = payment.PeoplesId;
                PersonName = payment.Person?.Nombre;
                Amount = payment.Amount;
                Comentary = payment.Comentary;
                Ano = payment.Ano;
                Mes = payment.Mes;
                Fecha = payment.Fecha;
                ComprobanteUrl = payment.ComprobanteUrl;
                
                // Mostrar comprobante existente
                if (!string.IsNullOrEmpty(ComprobanteUrl))
                {
                    ComprobantePreview = ImageSource.FromUri(new Uri(ComprobanteUrl));
                    ComprobanteFileName = Path.GetFileName(new Uri(ComprobanteUrl).LocalPath);
                    HasComprobante = true;
                }
                
                Title = "Editar Pago";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading payment: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Error al cargar pago: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Seleccionar comprobante"
            });

            if (result != null)
            {
                // Leer la imagen
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                _comprobanteData = memoryStream.ToArray();

                // Mostrar preview
                ComprobantePreview = ImageSource.FromStream(() => new MemoryStream(_comprobanteData));
                ComprobanteFileName = result.FileName;
                HasComprobante = true;

                System.Diagnostics.Debug.WriteLine($"✅ Imagen seleccionada: {result.FileName}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error selecting image: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Error al seleccionar imagen: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validaciones
        if (PersonId == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Debe seleccionar una persona", "OK");
            return;
        }

        if (Amount <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "El monto debe ser mayor a 0", "OK");
            return;
        }

        if (Mes < 1 || Mes > 12)
        {
            await Shell.Current.DisplayAlert("Error", "El mes debe estar entre 1 y 12", "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // ✅ CORREGIDO: Subir comprobante si hay uno nuevo seleccionado
            if (_comprobanteData != null && !string.IsNullOrEmpty(ComprobanteFileName))
            {
                System.Diagnostics.Debug.WriteLine($"📤 Subiendo comprobante: {ComprobanteFileName}");
                
                using var stream = new MemoryStream(_comprobanteData);
                ComprobanteUrl = await _blobStorageService.UploadImageAsync(
                    stream, 
                    ComprobanteFileName, 
                    "comprobantes"
                );
                
                System.Diagnostics.Debug.WriteLine($"✅ Comprobante subido: {ComprobanteUrl}");
            }

            var payment = new Payment
            {
                Id = Id,
                PeoplesId = PersonId,
                Amount = Amount,
                Comentary = Comentary,
                Ano = Ano,
                Mes = Mes,
                Fecha = Fecha,
                ComprobanteUrl = ComprobanteUrl
            };

            if (Id == 0)
            {
                await _apiService.CreatePaymentAsync(payment);
                await Shell.Current.DisplayAlert("Éxito", "Pago creado correctamente", "OK");
            }
            else
            {
                await _apiService.UpdatePaymentAsync(payment);
                await Shell.Current.DisplayAlert("Éxito", "Pago actualizado correctamente", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error saving payment: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}