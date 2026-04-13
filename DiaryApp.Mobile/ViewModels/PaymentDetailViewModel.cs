using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Shared.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
[QueryProperty(nameof(PersonId), nameof(PersonId))]
public partial class PaymentDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService; // ✅ Cambiar de IDatabaseService

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

    public PaymentDetailViewModel(IApiService apiService) // ✅ Cambiar parámetro
    {
        _apiService = apiService;
        Title = "Detalle Pago";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            LoadPaymentAsync(value).ConfigureAwait(false);
        }
    }

    private async Task LoadPaymentAsync(int paymentId)
    {
        try
        {
            var payment = await _apiService.GetPaymentAsync(paymentId); // ✅ Usar API
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
            await Shell.Current.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (PersonId == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Seleccione una persona", "OK");
            return;
        }

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

            if (Id == 0)
            {
                await _apiService.CreatePaymentAsync(payment); // ✅ Usar API
            }
            else
            {
                await _apiService.UpdatePaymentAsync(payment); // ✅ Usar API
            }

            await Shell.Current.DisplayAlert("Éxito", "Pago guardado", "OK");
            await Shell.Current.GoToAsync("..");
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
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}