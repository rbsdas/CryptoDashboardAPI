namespace CryptoDashboardAPI.DTOs.Watchlist;

public class WatchlistItemResponse
{
    public Guid Id { get; set; }
    public Guid CryptocurrencyId { get; set; }
    public string CoinName { get; set; } = string.Empty;
    public string CoinSymbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public DateTime AddedAt { get; set; }
}
