using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Models;
using DiaryApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace DiaryApp.Mobile.ViewModels;

public partial class DiaryEntriesViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<DiaryEntry> diaryEntries = [];

    public DiaryEntriesViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Diary Entries";
    }

    [RelayCommand]
    private async Task LoadEntriesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var items = await _databaseService.GetDiaryEntriesAsync();
            DiaryEntries.Clear();
            foreach (var item in items)
            {
                DiaryEntries.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error loading entries: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddEntryAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.DiaryEntryDetailPage));
    }

    [RelayCommand]
    private async Task GoToDetailAsync(DiaryEntry entry)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.DiaryEntryDetailPage)}?Id={entry.Id}");
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(DiaryEntry entry)
    {
        var confirm = await Shell.Current.DisplayAlert("Confirm", 
            $"Delete '{entry.Title}'?", "Yes", "No");
        
        if (confirm)
        {
            await _databaseService.DeleteDiaryEntryAsync(entry);
            await LoadEntriesAsync();
        }
    }
}