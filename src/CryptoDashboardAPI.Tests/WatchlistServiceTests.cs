using CryptoDashboardAPI.DTOs.Watchlist;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;
using CryptoDashboardAPI.Services;
using FluentAssertions;
using Moq;

namespace CryptoDashboardAPI.Tests;

public class WatchlistServiceTests
{
    private readonly Mock<IWatchlistRepository> _watchlistRepoMock = new();
    private readonly Mock<ICryptoRepository> _cryptoRepoMock = new();
    private readonly WatchlistService _sut;

    public WatchlistServiceTests()
    {
        _sut = new WatchlistService(_watchlistRepoMock.Object, _cryptoRepoMock.Object);
    }

    [Fact]
    public async Task AddAsync_CoinNotFound_ThrowsNotFoundException()
    {
        _cryptoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cryptocurrency?)null);

        var act = () => _sut.AddAsync(Guid.NewGuid(), new AddWatchlistRequest
        {
            CryptocurrencyId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_AlreadyInWatchlist_ThrowsConflictException()
    {
        var coinId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _cryptoRepoMock.Setup(r => r.GetByIdAsync(coinId))
            .ReturnsAsync(new Cryptocurrency { Id = coinId, Name = "Bitcoin", Symbol = "BTC" });
        _watchlistRepoMock.Setup(r => r.ExistsAsync(userId, coinId)).ReturnsAsync(true);

        var act = () => _sut.AddAsync(userId, new AddWatchlistRequest { CryptocurrencyId = coinId });

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already in your watchlist*");
    }

    [Fact]
    public async Task AddAsync_ValidRequest_ReturnsCorrectDto()
    {
        var coinId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var coin = new Cryptocurrency { Id = coinId, Name = "Ethereum", Symbol = "ETH" };

        _cryptoRepoMock.Setup(r => r.GetByIdAsync(coinId)).ReturnsAsync(coin);
        _watchlistRepoMock.Setup(r => r.ExistsAsync(userId, coinId)).ReturnsAsync(false);
        _watchlistRepoMock.Setup(r => r.AddAsync(It.IsAny<WatchlistItem>()))
            .ReturnsAsync((WatchlistItem item) => item);

        var result = await _sut.AddAsync(userId, new AddWatchlistRequest { CryptocurrencyId = coinId });

        result.CryptocurrencyId.Should().Be(coinId);
        result.CoinName.Should().Be("Ethereum");
        result.CoinSymbol.Should().Be("ETH");
        result.Id.Should().NotBeEmpty();
        result.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAsync_ReturnsWatchlistWithCoinDetails()
    {
        var userId = Guid.NewGuid();
        var coin = new Cryptocurrency
        {
            Id = Guid.NewGuid(),
            Name = "Solana",
            Symbol = "SOL",
            CurrentPrice = 150m,
            PriceChangePercentage = 2.5m
        };
        var items = new List<WatchlistItem>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, CryptocurrencyId = coin.Id, Cryptocurrency = coin, AddedAt = DateTime.UtcNow }
        };
        _watchlistRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(items);

        var result = await _sut.GetAsync(userId);

        result.Data.Should().HaveCount(1);
        result.Data.First().CoinName.Should().Be("Solana");
        result.Data.First().CurrentPrice.Should().Be(150m);
    }
}
