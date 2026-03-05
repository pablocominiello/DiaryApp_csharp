using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class DiaryEntriesPage : ContentPage
{
	private readonly DiaryEntriesViewModel _viewModel;

	public DiaryEntriesPage(DiaryEntriesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadEntriesCommand.ExecuteAsync(null);
	}
}