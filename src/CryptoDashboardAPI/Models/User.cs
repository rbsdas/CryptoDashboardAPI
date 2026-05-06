namespace CryptoDashboardAPI.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
}
