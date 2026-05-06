namespace CryptoDashboardAPI.Models;

public class WatchlistItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CryptocurrencyId { get; set; }
    public DateTime AddedAt { get; set; }

    public User User { get; set; } = null!;
    public Cryptocurrency Cryptocurrency { get; set; } = null!;
}
