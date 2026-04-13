namespace DiaryApp.Mobile.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    string? GetUserEmail();
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? UserId { get; set; }
    public string? ErrorMessage { get; set; }
}