using CryptoDashboardAPI.Data;
using CryptoDashboardAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboardAPI.Repositories;

public class CryptoRepository : ICryptoRepository
{
    private readonly AppDbContext _context;

    public CryptoRepository(AppDbContext context) => _context = context;

    public async Task<(IEnumerable<Cryptocurrency> Items, int Total)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Cryptocurrencies.AsNoTracking().OrderByDescending(c => c.MarketCap);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<Cryptocurrency?> GetByIdAsync(Guid id) =>
        _context.Cryptocurrencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsByIdAsync(Guid id) =>
        _context.Cryptocurrencies.AnyAsync(c => c.Id == id);

    public async Task UpsertManyAsync(IEnumerable<Cryptocurrency> coins)
    {
        var coinList = coins.ToList();
        var externalIds = coinList.Select(c => c.ExternalProviderId).ToList();

        var existing = await _context.Cryptocurrencies
            .Where(c => externalIds.Contains(c.ExternalProviderId))
            .ToDictionaryAsync(c => c.ExternalProviderId);

        foreach (var coin in coinList)
        {
            if (existing.TryGetValue(coin.ExternalProviderId, out var dbCoin))
            {
                dbCoin.Name = coin.Name;
                dbCoin.Symbol = coin.Symbol;
                dbCoin.CurrentPrice = coin.CurrentPrice;
                dbCoin.MarketCap = coin.MarketCap;
                dbCoin.PriceChangePercentage = coin.PriceChangePercentage;
                dbCoin.LastUpdatedAt = coin.LastUpdatedAt;
            }
            else
            {
                coin.Id = Guid.NewGuid();
                _context.Cryptocurrencies.Add(coin);
            }
        }

        await _context.SaveChangesAsync();
    }
}
