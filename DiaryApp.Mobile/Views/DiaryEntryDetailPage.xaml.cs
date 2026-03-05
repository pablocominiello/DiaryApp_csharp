using DiaryApp.Mobile.ViewModels;

namespace DiaryApp.Mobile.Views;

public partial class DiaryEntryDetailPage : ContentPage
{
	public DiaryEntryDetailPage(DiaryEntryDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}