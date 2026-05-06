using System.Text.Json.Serialization;

namespace CryptoDashboardAPI.DTOs.External;

public class CoinGeckoMarketDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("current_price")]
    public decimal? CurrentPrice { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal? MarketCap { get; set; }

    [JsonPropertyName("price_change_percentage_24h")]
    public decimal? PriceChangePercentage24h { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTime? LastUpdated { get; set; }
}
