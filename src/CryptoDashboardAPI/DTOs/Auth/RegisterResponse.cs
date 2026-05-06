namespace CryptoDashboardAPI.DTOs.Auth;

public class RegisterResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}
