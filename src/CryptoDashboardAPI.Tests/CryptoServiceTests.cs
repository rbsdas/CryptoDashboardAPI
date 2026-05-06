using System.Reflection;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;
using CryptoDashboardAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CryptoDashboardAPI.Tests;

public class CryptoServiceTests : IDisposable
{
    private readonly Mock<ICryptoRepository> _cryptoRepoMock = new();
    private readonly Mock<ICoinGeckoClient> _coinGeckoMock = new();
    private readonly CryptoService _sut;

    private static readonly FieldInfo LastRefreshedField =
        typeof(CryptoService).GetField("_lastRefreshedAt", BindingFlags.NonPublic | BindingFlags.Static)!;

    public CryptoServiceTests()
    {
        LastRefreshedField.SetValue(null, null);
        _sut = new CryptoService(_cryptoRepoMock.Object, _coinGeckoMock.Object, NullLogger<CryptoService>.Instance);
    }

    public void Dispose() => LastRefreshedField.SetValue(null, null);

    [Fact]
    public async Task GetAllAsync_ReturnsMappedPagedResult()
    {
        var coins = Enumerable.Range(1, 5).Select(i => new Cryptocurrency
        {
            Id = Guid.NewGuid(),
            Name = $"Coin{i}",
            Symbol = $"C{i}",
            ExternalProviderId = $"coin{i}",
            CurrentPrice = i * 100m
        }).ToList();

        _cryptoRepoMock.Setup(r => r.GetAllPagedAsync(2, 3)).ReturnsAsync((coins.Skip(3).Take(3), 5));

        var result = await _sut.GetAllAsync(2, 3);

        result.Total.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFoundException()
    {
        _cryptoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cryptocurrency?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsMappedDto()
    {
        var coin = new Cryptocurrency
        {
            Id = Guid.NewGuid(),
            Name = "Bitcoin",
            Symbol = "BTC",
            ExternalProviderId = "bitcoin",
            CurrentPrice = 80000m
        };
        _cryptoRepoMock.Setup(r => r.GetByIdAsync(coin.Id)).ReturnsAsync(coin);

        var result = await _sut.GetByIdAsync(coin.Id);

        result.Id.Should().Be(coin.Id);
        result.Name.Should().Be("Bitcoin");
        result.Symbol.Should().Be("BTC");
        result.CurrentPrice.Should().Be(80000m);
    }

    [Fact]
    public async Task RefreshAsync_WithinCooldown_ThrowsCooldownExceptionWithPositiveRetryAfter()
    {
        LastRefreshedField.SetValue(null, DateTime.UtcNow.AddSeconds(-10));

        var act = () => _sut.RefreshAsync();

        var ex = await act.Should().ThrowAsync<CooldownException>();
        ex.Which.RetryAfterSeconds.Should().BePositive();
    }

    [Fact]
    public async Task GetHistoryAsync_InvalidDays_ThrowsArgumentException()
    {
        var act = () => _sut.GetHistoryAsync(Guid.NewGuid(), 999);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid days value*");
    }

    [Fact]
    public async Task GetHistoryAsync_UnknownCoin_ThrowsNotFoundException()
    {
        _cryptoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cryptocurrency?)null);

        var act = () => _sut.GetHistoryAsync(Guid.NewGuid(), 7);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
