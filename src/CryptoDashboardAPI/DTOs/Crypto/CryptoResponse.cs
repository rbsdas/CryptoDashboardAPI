namespace CryptoDashboardAPI.DTOs.Crypto;

public class CryptoResponse
{
    public Guid Id { get; set; }
    public string ExternalProviderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
