using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoDashboardAPI.DTOs.Auth;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace CryptoDashboardAPI.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        var existing = await _userRepository.FindByEmailAsync(email);
        if (existing != null)
            throw new ConflictException($"Email '{email}' is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return new RegisterResponse { Id = user.Id, Email = user.Email };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email.ToLowerInvariant());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var expiresAt = DateTime.UtcNow.AddHours(GetTokenExpiryHours());
        var token = GenerateJwt(user, expiresAt);

        return new LoginResponse { Token = token, ExpiresAt = expiresAt };
    }

    private string GenerateJwt(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetJwtSecret() =>
        _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");

    private int GetTokenExpiryHours() =>
        int.TryParse(_configuration["Jwt:ExpiryHours"], out var hours) ? hours : 24;
}
