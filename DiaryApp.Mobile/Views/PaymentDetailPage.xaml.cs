using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class PaymentDetailPage : ContentPage
{
    private readonly PaymentDetailViewModel _viewModel;

    public PaymentDetailPage(PaymentDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}