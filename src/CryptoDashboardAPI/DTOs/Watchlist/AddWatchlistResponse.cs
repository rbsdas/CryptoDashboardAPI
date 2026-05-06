namespace CryptoDashboardAPI.DTOs.Watchlist;

public class AddWatchlistResponse
{
    public Guid Id { get; set; }
    public Guid CryptocurrencyId { get; set; }
    public string CoinName { get; set; } = string.Empty;
    public string CoinSymbol { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
