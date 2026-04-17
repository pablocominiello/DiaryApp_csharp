namespace DiaryApp.Mobile.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    string? GetUserEmail();
    Task<string?> GetUserIdAsync();
    Task<int?> GetPersonIdAsync();
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? UserId { get; set; }
    public int? PersonId { get; set; }
    public string? ErrorMessage { get; set; }
}