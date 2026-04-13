using System.Net.Http.Json;
using System.Text.Json;

namespace DiaryApp.Mobile.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string EmailKey = "user_email";
    private const string UserIdKey = "user_id";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    // Guardar token y datos de forma segura
                    await SecureStorage.SetAsync(TokenKey, result.Token);
                    await SecureStorage.SetAsync(EmailKey, result.Email);
                    await SecureStorage.SetAsync(UserIdKey, result.UserId);

                    return new AuthResult
                    {
                        Success = true,
                        Token = result.Token,
                        Email = result.Email,
                        UserId = result.UserId
                    };
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Error de autenticación: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(EmailKey);
        SecureStorage.Remove(UserIdKey);
        await Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserEmail()
    {
        try
        {
            return SecureStorage.GetAsync(EmailKey).Result;
        }
        catch
        {
            return null;
        }
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}