using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;
using DiaryApp.Shared.Models; // ✅ Usar el modelo compartido
using System.Collections.ObjectModel;

namespace DiaryApp.Mobile.ViewModels;

public partial class PersonsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Person> persons = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    public PersonsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "Personas";
    }

    [RelayCommand]
    private async Task LoadPersonsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var items = await _apiService.GetPersonsAsync(SearchText);
            Persons.Clear();
            foreach (var item in items)
            {
                Persons.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las personas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadPersonsAsync();
    }

    [RelayCommand]
    private async Task AddPersonAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.PersonDetailPage));
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Person person)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.PersonDetailPage)}?Id={person.Id}");
    }

    [RelayCommand]
    private async Task DeletePersonAsync(Person person)
    {
        var confirm = await Shell.Current.DisplayAlert("Confirmar", 
            $"¿Desea eliminar a {person.Nombre}?", "Sí", "No");
        
        if (confirm)
        {
            await _apiService.DeletePersonAsync(person.Id);
            await LoadPersonsAsync();
        }
    }
}