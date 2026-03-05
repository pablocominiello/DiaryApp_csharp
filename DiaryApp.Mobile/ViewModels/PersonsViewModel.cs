using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;
using DiaryApp.Shared.Models;
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
        try
        {
            // ✅ Navegar sin parámetro Id para crear nueva persona
            await Shell.Current.GoToAsync(nameof(Views.PersonDetailPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Navigation Error (Add): {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Person person)
    {
        if (person == null)
            return;

        try
        {
            // ✅ Pasar solo el Id como parámetro de navegación
            var route = $"{nameof(Views.PersonDetailPage)}?Id={person.Id}";
            System.Diagnostics.Debug.WriteLine($"🔍 Navigating to: {route}");
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Navigation Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeletePersonAsync(Person person)
    {
        if (person == null)
            return;

        try
        {
            var confirm = await Shell.Current.DisplayAlert("Confirmar", 
                $"¿Desea eliminar a {person.Nombre}?", "Sí", "No");
            
            if (confirm)
            {
                IsBusy = true;
                await _apiService.DeletePersonAsync(person.Id);
                await LoadPersonsAsync();
                await Shell.Current.DisplayAlert("Éxito", "Persona eliminada correctamente", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Delete Error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}