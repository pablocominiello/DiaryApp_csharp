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

    public PaymentDetailViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Detalle Pago";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadPaymentAsync(value);
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
            await Shell.Current.DisplayAlert("Error", $"Error al cargar pago: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
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