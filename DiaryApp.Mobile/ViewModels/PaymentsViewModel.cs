using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Models;
using DiaryApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(PersonId), nameof(PersonId))]
public partial class PaymentsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private int? personId;

    [ObservableProperty]
    private ObservableCollection<Payment> payments = [];

    public PaymentsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Pagos";
    }

    partial void OnPersonIdChanged(int? value)
    {
        LoadPaymentsAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task LoadPaymentsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var items = await _databaseService.GetPaymentsAsync(PersonId);
            Payments.Clear();
            foreach (var item in items)
            {
                Payments.Add(item);
            }
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

    [RelayCommand]
    private async Task AddPaymentAsync()
    {
        var route = PersonId.HasValue 
            ? $"{nameof(Views.PaymentDetailPage)}?PersonId={PersonId}" 
            : nameof(Views.PaymentDetailPage);
        await Shell.Current.GoToAsync(route);
    }

    [RelayCommand]
    private async Task DeletePaymentAsync(Payment payment)
    {
        var confirm = await Shell.Current.DisplayAlert("Confirmar", 
            "¿Eliminar este pago?", "Sí", "No");
        
        if (confirm)
        {
            await _databaseService.DeletePaymentAsync(payment);
            await LoadPaymentsAsync();
        }
    }
}