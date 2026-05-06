using CryptoDashboardAPI.DTOs.Watchlist;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;

namespace CryptoDashboardAPI.Services;

public class WatchlistService
{
    private readonly IWatchlistRepository _watchlistRepository;
    private readonly ICryptoRepository _cryptoRepository;

    public WatchlistService(IWatchlistRepository watchlistRepository, ICryptoRepository cryptoRepository)
    {
        _watchlistRepository = watchlistRepository;
        _cryptoRepository = cryptoRepository;
    }

    public async Task<AddWatchlistResponse> AddAsync(Guid userId, AddWatchlistRequest request)
    {
        var coin = await _cryptoRepository.GetByIdAsync(request.CryptocurrencyId);
        if (coin == null)
            throw new NotFoundException($"Cryptocurrency with id '{request.CryptocurrencyId}' not found.");

        var exists = await _watchlistRepository.ExistsAsync(userId, request.CryptocurrencyId);
        if (exists)
            throw new ConflictException("This cryptocurrency is already in your watchlist.");

        var item = new WatchlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CryptocurrencyId = request.CryptocurrencyId,
            AddedAt = DateTime.UtcNow
        };

        await _watchlistRepository.AddAsync(item);

        return new AddWatchlistResponse
        {
            Id = item.Id,
            CryptocurrencyId = item.CryptocurrencyId,
            CoinName = coin.Name,
            CoinSymbol = coin.Symbol,
            AddedAt = item.AddedAt
        };
    }

    public async Task<WatchlistResponse> GetAsync(Guid userId)
    {
        var items = await _watchlistRepository.GetByUserIdAsync(userId);

        return new WatchlistResponse
        {
            Data = items.Select(item => new WatchlistItemResponse
            {
                Id = item.Id,
                CryptocurrencyId = item.CryptocurrencyId,
                CoinName = item.Cryptocurrency.Name,
                CoinSymbol = item.Cryptocurrency.Symbol,
                CurrentPrice = item.Cryptocurrency.CurrentPrice,
                PriceChangePercentage = item.Cryptocurrency.PriceChangePercentage,
                AddedAt = item.AddedAt
            })
        };
    }
}
