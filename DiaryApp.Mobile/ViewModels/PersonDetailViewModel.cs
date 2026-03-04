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

    // ✅ Nueva propiedad para almacenar la imagen temporalmente
    [ObservableProperty]
    private byte[]? pendingImageBytes;

    [ObservableProperty]
    private string? pendingImageFileName;

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

            // ✅ Flujo diferenciado para crear vs editar
            if (IsEditing)
            {
                // Editar: Si hay imagen pendiente, subirla primero
                if (PendingImageBytes != null && !string.IsNullOrEmpty(PendingImageFileName))
                {
                    var base64Image = Convert.ToBase64String(PendingImageBytes);
                    person.ImagenUrl = await _apiService.UploadPersonImageAsync(Id, base64Image, PendingImageFileName);
                    ImagenUrl = person.ImagenUrl;
                    PendingImageBytes = null;
                    PendingImageFileName = null;
                }

                await _apiService.UpdatePersonAsync(person);
            }
            else
            {
                // Crear: Primero crear la persona, luego subir la imagen si existe
                var createdPerson = await _apiService.CreatePersonAsync(person);
                
                if (createdPerson != null && createdPerson.Id > 0)
                {
                    // Si hay imagen pendiente, subirla ahora que tenemos el ID
                    if (PendingImageBytes != null && !string.IsNullOrEmpty(PendingImageFileName))
                    {
                        try
                        {
                            var base64Image = Convert.ToBase64String(PendingImageBytes);
                            var uploadedUrl = await _apiService.UploadPersonImageAsync(createdPerson.Id, base64Image, PendingImageFileName);
                            
                            // Actualizar la persona con la URL de la imagen
                            createdPerson.ImagenUrl = uploadedUrl;
                            await _apiService.UpdatePersonAsync(createdPerson);
                            
                            PendingImageBytes = null;
                            PendingImageFileName = null;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al subir imagen: {ex.Message}");
                            // La persona se creó pero la imagen falló - no es crítico
                        }
                    }
                }
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
            // ✅ Solicitar permisos explícitamente
            var status = await Permissions.CheckStatusAsync<Permissions.Media>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Media>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Permiso Denegado", 
                        "No se puede acceder a las fotos sin los permisos necesarios.", "OK");
                    return;
                }
            }

            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona una foto"
            });

            if (result != null)
            {
                // ✅ Guardar imagen temporalmente
                var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                PendingImageBytes = memoryStream.ToArray();
                PendingImageFileName = result.FileName;

                // ✅ Mostrar preview local convirtiendo a base64
                var base64 = Convert.ToBase64String(PendingImageBytes);
                ImagenUrl = $"data:image/jpeg;base64,{base64}";

                // ✅ Si estamos editando (Id > 0), subir inmediatamente
                if (IsEditing && Id > 0)
                {
                    try
                    {
                        IsBusy = true;
                        var uploadedUrl = await _apiService.UploadPersonImageAsync(Id, base64, result.FileName);
                        ImagenUrl = uploadedUrl;
                        PendingImageBytes = null;
                        PendingImageFileName = null;
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Advertencia", 
                            "La imagen se mostrará pero se subirá al guardar", "OK");
                        System.Diagnostics.Debug.WriteLine($"Error al subir imagen: {ex.Message}");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Exception en PickImageAsync: {ex}");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}