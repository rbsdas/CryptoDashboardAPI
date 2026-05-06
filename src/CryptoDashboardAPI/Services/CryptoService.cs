using CryptoDashboardAPI.DTOs.Crypto;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Models;
using CryptoDashboardAPI.Repositories;

namespace CryptoDashboardAPI.Services;

public class CryptoService
{
    private readonly ICryptoRepository _cryptoRepository;
    private readonly ICoinGeckoClient _coinGeckoClient;
    private readonly ILogger<CryptoService> _logger;

    private static DateTime? _lastRefreshedAt;
    private const int CooldownSeconds = 60;
    private static readonly HashSet<int> AllowedHistoryDays = new() { 1, 7, 14, 30, 90, 180, 365 };

    public CryptoService(ICryptoRepository cryptoRepository, ICoinGeckoClient coinGeckoClient, ILogger<CryptoService> logger)
    {
        _cryptoRepository = cryptoRepository;
        _coinGeckoClient = coinGeckoClient;
        _logger = logger;
    }

    public async Task<CryptoListResponse> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _cryptoRepository.GetAllPagedAsync(page, pageSize);
        return new CryptoListResponse
        {
            Data = items.Select(MapToResponse),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CryptoResponse> GetByIdAsync(Guid id)
    {
        var coin = await _cryptoRepository.GetByIdAsync(id);
        if (coin == null) throw new NotFoundException($"Cryptocurrency with id '{id}' not found.");
        return MapToResponse(coin);
    }

    public async Task<RefreshResponse> RefreshAsync()
    {
        if (_lastRefreshedAt.HasValue)
        {
            var elapsed = (DateTime.UtcNow - _lastRefreshedAt.Value).TotalSeconds;
            if (elapsed < CooldownSeconds)
            {
                var retryAfter = (int)(CooldownSeconds - elapsed) + 1;
                throw new CooldownException("Refresh is on cooldown. Please try again later.", retryAfter);
            }
        }

        var markets = await _coinGeckoClient.GetMarketsAsync();

        var coins = markets.Select(m => new Cryptocurrency
        {
            ExternalProviderId = m.Id,
            Name = m.Name,
            Symbol = m.Symbol.ToUpperInvariant(),
            CurrentPrice = m.CurrentPrice ?? 0,
            MarketCap = m.MarketCap ?? 0,
            PriceChangePercentage = m.PriceChangePercentage24h ?? 0,
            LastUpdatedAt = m.LastUpdated?.ToUniversalTime() ?? DateTime.UtcNow
        }).ToList();

        await _cryptoRepository.UpsertManyAsync(coins);

        _lastRefreshedAt = DateTime.UtcNow;
        _logger.LogInformation("Refreshed {Count} coins from CoinGecko", coins.Count);

        return new RefreshResponse { RefreshedCount = coins.Count, RefreshedAt = _lastRefreshedAt.Value };
    }

    public async Task<CryptoHistoryResponse> GetHistoryAsync(Guid id, int days)
    {
        if (!AllowedHistoryDays.Contains(days))
            throw new ArgumentException($"Invalid days value. Allowed: {string.Join(", ", AllowedHistoryDays)}.");

        var coin = await _cryptoRepository.GetByIdAsync(id);
        if (coin == null) throw new NotFoundException($"Cryptocurrency with id '{id}' not found.");

        var chart = await _coinGeckoClient.GetMarketChartAsync(coin.ExternalProviderId, days);

        var prices = chart.Prices.Select(p => new PricePoint
        {
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)p[0].GetDouble()).UtcDateTime,
            Price = (decimal)p[1].GetDouble()
        }).ToList();

        return new CryptoHistoryResponse
        {
            CoinId = coin.Id,
            ExternalProviderId = coin.ExternalProviderId,
            Days = days,
            Prices = prices
        };
    }

    private static CryptoResponse MapToResponse(Cryptocurrency coin) => new()
    {
        Id = coin.Id,
        ExternalProviderId = coin.ExternalProviderId,
        Name = coin.Name,
        Symbol = coin.Symbol,
        CurrentPrice = coin.CurrentPrice,
        MarketCap = coin.MarketCap,
        PriceChangePercentage = coin.PriceChangePercentage,
        LastUpdatedAt = coin.LastUpdatedAt
    };
}
