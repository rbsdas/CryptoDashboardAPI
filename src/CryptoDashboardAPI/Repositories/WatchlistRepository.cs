using CryptoDashboardAPI.Data;
using CryptoDashboardAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboardAPI.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly AppDbContext _context;

    public WatchlistRepository(AppDbContext context) => _context = context;

    public Task<bool> ExistsAsync(Guid userId, Guid cryptocurrencyId) =>
        _context.WatchlistItems.AnyAsync(w => w.UserId == userId && w.CryptocurrencyId == cryptocurrencyId);

    public async Task<WatchlistItem> AddAsync(WatchlistItem item)
    {
        _context.WatchlistItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<IEnumerable<WatchlistItem>> GetByUserIdAsync(Guid userId)
    {
        return await _context.WatchlistItems
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Include(w => w.Cryptocurrency)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();
    }
}
