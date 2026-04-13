using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiaryApp.Mobile.Services;

namespace DiaryApp.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Iniciar Sesión";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor ingresa tu email";
            HasError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingresa tu contraseña";
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
                // Login exitoso - navegar a la app principal
                await Shell.Current.GoToAsync("///persons");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Error al iniciar sesión";
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
}