using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class PersonDetailPage : ContentPage
{
	public PersonDetailPage(PersonDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}