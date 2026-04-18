using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Shared.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
[QueryProperty(nameof(PersonId), nameof(PersonId))]
public partial class PaymentDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int personId;

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
    private bool isEditing;

    [ObservableProperty]
    private byte[]? pendingImageBytes;

    [ObservableProperty]
    private string? pendingImageFileName;

    public PaymentDetailViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Nuevo Pago";
    }

    public async Task InitializeAsync()
    {
        // ✅ NUEVO: Verificar si hay una imagen compartida pendiente
        #if ANDROID
        await LoadSharedImageIfExistsAsync();
        #endif

        if (Id > 0)
        {
            IsEditing = true;
            await LoadPaymentAsync(Id);
        }
        else
        {
            IsEditing = false;
            Title = "Nuevo Pago";
            
            // ✅ Cargar PersonId automáticamente si el usuario está logueado
            if (PersonId == 0)
            {
                var currentPersonId = await _authService.GetPersonIdAsync();
                if (currentPersonId.HasValue)
                {
                    PersonId = currentPersonId.Value;
                }
            }
        }
    }

    #if ANDROID
    private async Task LoadSharedImageIfExistsAsync()
    {
        try
        {
            var imageBytes = await SharedImageHandler.GetSharedImageBytesAsync();
            
            if (imageBytes != null && imageBytes.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Loaded shared image: {imageBytes.Length} bytes");
                
                PendingImageBytes = imageBytes;
                PendingImageFileName = SharedImageHandler.GetSharedImageFileName();
                
                // Mostrar preview
                var base64 = Convert.ToBase64String(imageBytes);
                ComprobanteUrl = $"data:image/jpeg;base64,{base64}";
                
                // Limpiar el handler
                SharedImageHandler.ClearSharedImage();
                
                await Shell.Current.DisplayAlert(
                    "✅ Imagen Cargada", 
                    "El comprobante compartido se ha cargado correctamente. Completa los datos del pago.", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading shared image: {ex.Message}");
        }
    }
    #endif

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
                Amount = payment.Amount;
                Comentary = payment.Comentary;
                Ano = payment.Ano;
                Mes = payment.Mes;
                Fecha = payment.Fecha;
                ComprobanteUrl = payment.ComprobanteUrl;
                Title = "Editar Pago";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading payment: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo cargar el pago", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validaciones
        if (PersonId <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "Seleccione una persona válida", "OK");
            return;
        }

        if (Amount <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "El monto debe ser mayor a 0", "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

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

            if (IsEditing)
            {
                // Subir imagen si hay una pendiente
                if (PendingImageBytes != null && !string.IsNullOrEmpty(PendingImageFileName))
                {
                    var base64Image = Convert.ToBase64String(PendingImageBytes);
                    // TODO: Implementar endpoint para subir imagen de comprobante
                    // payment.ComprobanteUrl = await _apiService.UploadComprobanteAsync(...);
                    PendingImageBytes = null;
                    PendingImageFileName = null;
                }

                await _apiService.UpdatePaymentAsync(payment);
            }
            else
            {
                var createdPayment = await _apiService.CreatePaymentAsync(payment);
                
                // Subir imagen después de crear el pago
                if (createdPayment != null && createdPayment.Id > 0)
                {
                    if (PendingImageBytes != null && !string.IsNullOrEmpty(PendingImageFileName))
                    {
                        try
                        {
                            var base64Image = Convert.ToBase64String(PendingImageBytes);
                            // TODO: Implementar endpoint para subir imagen de comprobante
                            // createdPayment.ComprobanteUrl = await _apiService.UploadComprobanteAsync(...);
                            // await _apiService.UpdatePaymentAsync(createdPayment);
                            
                            PendingImageBytes = null;
                            PendingImageFileName = null;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Error uploading image: {ex.Message}");
                        }
                    }
                }
            }

            await Shell.Current.DisplayAlert("✅ Éxito", "Pago guardado correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar el pago: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona un comprobante"
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                PendingImageBytes = memoryStream.ToArray();
                PendingImageFileName = result.FileName;

                var base64 = Convert.ToBase64String(PendingImageBytes);
                ComprobanteUrl = $"data:image/jpeg;base64,{base64}";
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}