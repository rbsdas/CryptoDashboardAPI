namespace CryptoDashboardAPI.DTOs.Watchlist;

public class WatchlistResponse
{
    public IEnumerable<WatchlistItemResponse> Data { get; set; } = Array.Empty<WatchlistItemResponse>();
}
