using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class DiaryEntryDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string entryTitle = string.Empty;

    [ObservableProperty]
    private string content = string.Empty;

    [ObservableProperty]
    private DateTime dateCreated = DateTime.Now;

    public DiaryEntryDetailViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Diary Entry";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            LoadEntryAsync(value).ConfigureAwait(false);
        }
    }

    private async Task LoadEntryAsync(int entryId)
    {
        var entry = await _databaseService.GetDiaryEntryAsync(entryId);
        if (entry != null)
        {
            EntryTitle = entry.Title;
            Content = entry.Content;
            DateCreated = entry.DateCreated;
            Title = "Edit Entry";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EntryTitle) || EntryTitle.Length < 3)
        {
            await Shell.Current.DisplayAlert("Error", "Title must be at least 3 characters", "OK");
            return;
        }

        var entry = new DiaryEntry
        {
            Id = Id,
            Title = EntryTitle,
            Content = Content,
            DateCreated = DateCreated
        };

        await _databaseService.SaveDiaryEntryAsync(entry);
        await Shell.Current.GoToAsync("..");
    }
}