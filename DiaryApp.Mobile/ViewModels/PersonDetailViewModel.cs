using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Models;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PersonDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
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

    public PersonDetailViewModel(IDatabaseService databaseService, IBlobStorageService blobStorageService)
    {
        _databaseService = databaseService;
        _blobStorageService = blobStorageService;
        Title = "Detalle Persona";
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            LoadPersonAsync(value).ConfigureAwait(false);
        }
    }

    private async Task LoadPersonAsync(int personId)
    {
        var person = await _databaseService.GetPersonAsync(personId);
        if (person != null)
        {
            Nombre = person.Nombre;
            Content = person.Content;
            Born = person.Born;
            ImagenUrl = person.ImagenUrl;
            Title = "Editar Persona";
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

        var person = new Person
        {
            Id = Id,
            Nombre = Nombre,
            Content = Content,
            Born = Born,
            ImagenUrl = ImagenUrl
        };

        await _databaseService.SavePersonAsync(person);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                ImagenUrl = await _blobStorageService.UploadImageAsync(stream, result.FileName);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
        }
    }
}