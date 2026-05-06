using CryptoDashboardAPI.Models;

namespace CryptoDashboardAPI.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User> AddAsync(User user);
}
