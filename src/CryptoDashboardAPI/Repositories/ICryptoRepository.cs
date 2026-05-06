using CryptoDashboardAPI.Models;

namespace CryptoDashboardAPI.Repositories;

public interface ICryptoRepository
{
    Task<(IEnumerable<Cryptocurrency> Items, int Total)> GetAllPagedAsync(int page, int pageSize);
    Task<Cryptocurrency?> GetByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
    Task UpsertManyAsync(IEnumerable<Cryptocurrency> coins);
}
