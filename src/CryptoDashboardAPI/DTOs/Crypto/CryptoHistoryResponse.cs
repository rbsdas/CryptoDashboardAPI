namespace CryptoDashboardAPI.DTOs.Crypto;

public class PricePoint
{
    public DateTime Timestamp { get; set; }
    public decimal Price { get; set; }
}

public class CryptoHistoryResponse
{
    public Guid CoinId { get; set; }
    public string ExternalProviderId { get; set; } = string.Empty;
    public int Days { get; set; }
    public IEnumerable<PricePoint> Prices { get; set; } = Array.Empty<PricePoint>();
}
