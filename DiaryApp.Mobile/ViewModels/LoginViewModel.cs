using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private const string RememberMeKey = "remember_me";
    private const string SavedEmailKey = "saved_email";

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Iniciar Sesion";
        
        // Cargar configuración de "Recordar usuario"
        LoadRememberedUserAsync();
    }

    private async void LoadRememberedUserAsync()
    {
        try
        {
            // Verificar si el usuario quiere ser recordado
            var rememberMeValue = await SecureStorage.GetAsync(RememberMeKey);
            RememberMe = rememberMeValue == "true";

            // Si está marcado, cargar el email guardado
            if (RememberMe)
            {
                var savedEmail = await SecureStorage.GetAsync(SavedEmailKey);
                if (!string.IsNullOrEmpty(savedEmail))
                {
                    Email = savedEmail;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading remembered user: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        // Validaciones basicas
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor ingresa tu email";
            HasError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingresa tu contrasena";
            HasError = true;
            return;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var result = await _authService.LoginAsync(Email, Password);

            if (result.Success)
            {
                // Guardar preferencia de "Recordar usuario"
                await SaveRememberMePreferenceAsync();

                // Login exitoso - navegar a la app principal
                await Shell.Current.GoToAsync("///persons");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Error al iniciar sesion";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveRememberMePreferenceAsync()
    {
        try
        {
            if (RememberMe)
            {
                // Guardar que el usuario quiere ser recordado
                await SecureStorage.SetAsync(RememberMeKey, "true");
                // Guardar el email
                await SecureStorage.SetAsync(SavedEmailKey, Email);
            }
            else
            {
                // Eliminar las preferencias guardadas
                SecureStorage.Remove(RememberMeKey);
                SecureStorage.Remove(SavedEmailKey);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving remember me preference: {ex.Message}");
        }
    }
}