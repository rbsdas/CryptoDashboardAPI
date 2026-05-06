using CryptoDashboardAPI.Data;
using CryptoDashboardAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboardAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public Task<User?> FindByEmailAsync(string email) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
