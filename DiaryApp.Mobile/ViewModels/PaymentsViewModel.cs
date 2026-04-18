using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;
using DiaryApp.Shared.Models;
using System.Collections.ObjectModel;

namespace DiaryApp.Mobile.ViewModels;

public partial class PaymentsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<Payment> payments = [];

    [ObservableProperty]
    private int? currentPersonId;

    [ObservableProperty]
    private string personName = string.Empty;

    public PaymentsViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Mis Pagos";
    }

    [RelayCommand]
    private async Task LoadPaymentsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // ✅ CORREGIDO: Usar GetPersonIdAsync() directamente
            if (!CurrentPersonId.HasValue)
            {
                // Primero intentar obtener el PersonId guardado en el login
                CurrentPersonId = await _authService.GetPersonIdAsync();
                
                // Si no existe, intentar obtenerlo del API usando el UserId
                if (!CurrentPersonId.HasValue)
                {
                    var userId = await _authService.GetUserIdAsync();
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ No UserId found - user not authenticated");
                        await Shell.Current.DisplayAlert("Error", "No estás autenticado. Por favor inicia sesión.", "OK");
                        await Shell.Current.GoToAsync("///login");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"🔍 Fetching Person for UserId: {userId}");
                    
                    // Obtener la persona asociada al usuario
                    var person = await _apiService.GetPersonByUserIdAsync(userId);
                    
                    if (person == null)
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ No Person found for current user");
                        await Shell.Current.DisplayAlert("Error", 
                            "No se encontró tu perfil de persona. Por favor contacta al administrador.", "OK");
                        return;
                    }

                    CurrentPersonId = person.Id;
                    PersonName = person.Nombre;
                }
                else
                {
                    // Si tenemos PersonId, obtener el nombre de la persona
                    var person = await _apiService.GetPersonAsync(CurrentPersonId.Value);
                    if (person != null)
                    {
                        PersonName = person.Nombre;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Current PersonId: {CurrentPersonId}, Name: {PersonName}");
            }

            // ✅ Cargar solo los pagos de la persona autenticada
            var items = await _apiService.GetPaymentsAsync(CurrentPersonId);
            
            Payments.Clear();
            foreach (var item in items)
            {
                Payments.Add(item);
            }

            System.Diagnostics.Debug.WriteLine($"✅ Loaded {Payments.Count} payments for PersonId: {CurrentPersonId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading payments: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Error al cargar pagos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddPaymentAsync()
    {
        if (!CurrentPersonId.HasValue)
        {
            await Shell.Current.DisplayAlert("Error", "No se puede crear un pago sin una persona asignada", "OK");
            return;
        }

        // ✅ Pasar el PersonId al crear un nuevo pago
        await Shell.Current.GoToAsync($"{nameof(Views.PaymentDetailPage)}?PersonId={CurrentPersonId}");
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Payment payment)
    {
        if (payment == null)
            return;

        await Shell.Current.GoToAsync($"{nameof(Views.PaymentDetailPage)}?Id={payment.Id}");
    }

    [RelayCommand]
    private async Task DeletePaymentAsync(Payment payment)
    {
        if (payment == null)
            return;

        var confirm = await Shell.Current.DisplayAlert("Confirmar",
            $"¿Eliminar pago de {payment.Amount:C} del {payment.Mes}/{payment.Ano}?", "Sí", "No");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                await _apiService.DeletePaymentAsync(payment.Id);
                await LoadPaymentsAsync();
                await Shell.Current.DisplayAlert("Éxito", "Pago eliminado correctamente", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deleting payment: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}