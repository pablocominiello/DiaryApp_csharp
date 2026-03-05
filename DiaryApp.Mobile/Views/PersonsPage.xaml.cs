using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class PersonsPage : ContentPage
{
	private readonly PersonsViewModel _viewModel;

	public PersonsPage(PersonsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadPersonsCommand.ExecuteAsync(null);
	}
}