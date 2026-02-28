using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Shared.Models; // ✅ Usar modelo compartido
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PersonDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IBlobStorageService _blobStorageService;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string content = string.Empty;

    [ObservableProperty]
    private DateTime born = DateTime.Now;

    [ObservableProperty]
    private string? imagenUrl;

    [ObservableProperty]
    private bool isEditing;

    public PersonDetailViewModel(IApiService apiService, IBlobStorageService blobStorageService)
    {
        _apiService = apiService;
        _blobStorageService = blobStorageService;
        Title = "Nueva Persona";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            IsEditing = true;
            _ = LoadPersonAsync(value); // ✅ Fire and forget
        }
        else
        {
            IsEditing = false;
            Title = "Nueva Persona";
        }
    }

    private async Task LoadPersonAsync(int personId)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var person = await _apiService.GetPersonAsync(personId);
            if (person != null)
            {
                Nombre = person.Nombre;
                Content = person.Content;
                Born = person.Born;
                ImagenUrl = person.ImagenUrl;
                Title = "Editar Persona";
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la persona: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre) || Nombre.Length < 3)
        {
            await Shell.Current.DisplayAlert("Error", "El nombre debe tener al menos 3 caracteres", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Content) || Content.Length < 3)
        {
            await Shell.Current.DisplayAlert("Error", "El contenido debe tener al menos 3 caracteres", "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var person = new Person
            {
                Id = Id,
                Nombre = Nombre,
                Content = Content,
                Born = Born,
                ImagenUrl = ImagenUrl
            };

            if (IsEditing)
            {
                await _apiService.UpdatePersonAsync(person);
            }
            else
            {
                await _apiService.CreatePersonAsync(person);
            }

            await Shell.Current.DisplayAlert("Éxito", "Persona guardada correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la persona: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona una foto"
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                // Subir a Azure Blob Storage
                ImagenUrl = await _blobStorageService.UploadImageAsync(stream, result.FileName, "persons");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}