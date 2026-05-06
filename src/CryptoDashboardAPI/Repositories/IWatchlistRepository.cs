using CryptoDashboardAPI.Models;

namespace CryptoDashboardAPI.Repositories;

public interface IWatchlistRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid cryptocurrencyId);
    Task<WatchlistItem> AddAsync(WatchlistItem item);
    Task<IEnumerable<WatchlistItem>> GetByUserIdAsync(Guid userId);
}
