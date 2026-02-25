using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
[QueryProperty(nameof(PersonId), nameof(PersonId))]
public partial class PaymentDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int personId;

    [ObservableProperty]
    private int ano = DateTime.Now.Year;

    [ObservableProperty]
    private int mes = DateTime.Now.Month;

    [ObservableProperty]
    private DateTime fecha = DateTime.Now;

    [ObservableProperty]
    private string? comprobanteUrl;

    public PaymentDetailViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
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
        var payment = await _databaseService.GetPaymentAsync(paymentId);
        if (payment != null)
        {
            PersonId = payment.PeoplesId;
            Ano = payment.Ano;
            Mes = payment.Mes;
            Fecha = payment.Fecha;
            ComprobanteUrl = payment.ComprobanteUrl;
            Title = "Editar Pago";
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

        var payment = new Payment
        {
            Id = Id,
            PeoplesId = PersonId,
            Ano = Ano,
            Mes = Mes,
            Fecha = Fecha,
            ComprobanteUrl = ComprobanteUrl
        };

        await _databaseService.SavePaymentAsync(payment);
        await Shell.Current.GoToAsync("..");
    }
}