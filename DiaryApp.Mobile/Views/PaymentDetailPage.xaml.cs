using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class PaymentDetailPage : ContentPage
{
	public PaymentDetailPage(PaymentDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}