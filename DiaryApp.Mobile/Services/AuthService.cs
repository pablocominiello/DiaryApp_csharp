using System.Net.Http.Json;
using System.Text.Json;

namespace DiaryApp.Mobile.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string EmailKey = "user_email";
    private const string UserIdKey = "user_id";
    private const string PersonIdKey = "person_id";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔐 Attempting login for: {email}");
            
            var request = new
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            System.Diagnostics.Debug.WriteLine($"📊 Login Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📄 Login Response: {content}");
                
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
                    
                    // ✅ NUEVO: Guardar PersonId si existe
                    if (result.PersonId.HasValue)
                    {
                        await SecureStorage.SetAsync(PersonIdKey, result.PersonId.Value.ToString());
                        System.Diagnostics.Debug.WriteLine($"✅ Stored PersonId: {result.PersonId.Value}");
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Login successful for: {result.Email}");

                    return new AuthResult
                    {
                        Success = true,
                        Token = result.Token,
                        Email = result.Email,
                        UserId = result.UserId,
                        PersonId = result.PersonId
                    };
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"❌ Login failed: {errorContent}");
            
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Error de autenticación: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Login exception: {ex.Message}");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔓 Logging out...");
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(EmailKey);
        SecureStorage.Remove(UserIdKey);
        SecureStorage.Remove(PersonIdKey);
        await Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        var isAuth = !string.IsNullOrEmpty(token);
        System.Diagnostics.Debug.WriteLine($"🔍 IsAuthenticated: {isAuth}");
        return isAuth;
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

    public async Task<string?> GetUserIdAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(UserIdKey);
        }
        catch
        {
            return null;
        }
    }

    // ✅ NUEVO: Método para obtener PersonId
    public async Task<int?> GetPersonIdAsync()
    {
        try
        {
            var personIdStr = await SecureStorage.GetAsync(PersonIdKey);
            if (int.TryParse(personIdStr, out var personId))
            {
                System.Diagnostics.Debug.WriteLine($"✅ Retrieved PersonId: {personId}");
                return personId;
            }
            System.Diagnostics.Debug.WriteLine("⚠️ PersonId not found in secure storage");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting PersonId: {ex.Message}");
            return null;
        }
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int? PersonId { get; set; }
    }
}