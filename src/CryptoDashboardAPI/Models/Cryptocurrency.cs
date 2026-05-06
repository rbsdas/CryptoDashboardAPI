namespace CryptoDashboardAPI.Models;

public class Cryptocurrency
{
    public Guid Id { get; set; }
    public string ExternalProviderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
}
