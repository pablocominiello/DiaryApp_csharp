using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;
using DiaryApp.Shared.Models;
using System.Collections.ObjectModel;

namespace DiaryApp.Mobile.ViewModels;

public partial class PaymentsViewModel : BaseViewModel
{
    private readonly IApiService _apiService; // ✅ Cambiar de IDatabaseService

    [ObservableProperty]
    private ObservableCollection<Payment> payments = [];

    [ObservableProperty]
    private int? selectedPersonId;

    public PaymentsViewModel(IApiService apiService) // ✅ Cambiar parámetro
    {
        _apiService = apiService;
        Title = "Pagos";
    }

    [RelayCommand]
    private async Task LoadPaymentsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var items = await _apiService.GetPaymentsAsync(SelectedPersonId); // ✅ Usar API
            Payments.Clear();
            foreach (var item in items)
            {
                Payments.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error loading payments: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddPaymentAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.PaymentDetailPage));
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
            $"¿Eliminar pago?", "Sí", "No");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                await _apiService.DeletePaymentAsync(payment.Id); // ✅ Usar API
                await LoadPaymentsAsync();
                await Shell.Current.DisplayAlert("Éxito", "Pago eliminado", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}