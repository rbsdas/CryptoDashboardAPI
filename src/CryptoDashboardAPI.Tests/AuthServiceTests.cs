using CryptoDashboardAPI.DTOs.Auth;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;
using CryptoDashboardAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CryptoDashboardAPI.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-that-is-at-least-32-chars!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryHours"] = "24"
            })
            .Build();

        _sut = new AuthService(_userRepoMock.Object, config);
    }

    [Fact]
    public async Task Register_NewEmail_ReturnsUserIdAndEmail()
    {
        _userRepoMock.Setup(r => r.FindByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "password123"
        });

        result.Id.Should().NotBeEmpty();
        result.Email.Should().Be("new@example.com");
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "new@example.com" && !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        _userRepoMock.Setup(r => r.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        var act = () => _sut.RegisterAsync(new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*existing@example.com*");
    }

    [Fact]
    public async Task Register_NormalisesEmailToLowercase()
    {
        _userRepoMock.Setup(r => r.FindByEmailAsync("user@example.com")).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "USER@EXAMPLE.COM",
            Password = "password123"
        });

        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndExpiry()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        _userRepoMock.Setup(r => r.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = hash });

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        });

        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        _userRepoMock.Setup(r => r.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Email = "user@example.com", PasswordHash = hash });

        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrongpassword"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        _userRepoMock.Setup(r => r.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "ghost@example.com",
            Password = "password123"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
