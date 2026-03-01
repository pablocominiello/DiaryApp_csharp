using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Shared.Models;
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
            _ = LoadPersonAsync(value);
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

                // ✅ Validar si la URL de imagen existe antes de asignarla
                if (!string.IsNullOrEmpty(person.ImagenUrl))
                {
                    // Verificar si es una URL válida
                    if (Uri.TryCreate(person.ImagenUrl, UriKind.Absolute, out var uri))
                    {
                        ImagenUrl = person.ImagenUrl;
                    }
                    else
                    {
                        // URL inválida, usar imagen por defecto o null
                        ImagenUrl = null;
                        System.Diagnostics.Debug.WriteLine($"URL de imagen inválida: {person.ImagenUrl}");
                    }
                }
                else
                {
                    ImagenUrl = null;
                }

                Title = "Editar Persona";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar persona: {ex.Message}");
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
                // Instead of uploading directly to blob storage,
                // send the image to your API which handles the upload
                var stream = await result.OpenReadAsync();
                
                // Option A: Convert to base64 and send in JSON
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);
                
                // Send to API endpoint that handles image upload
                // This way your API manages Azure Blob Storage
                ImagenUrl = await _apiService.UploadPersonImageAsync(Id, base64Image, result.FileName);
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