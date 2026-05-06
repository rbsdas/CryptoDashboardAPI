using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoDashboardAPI.DTOs.External;

public class CoinGeckoChartDto
{
    [JsonPropertyName("prices")]
    public List<List<JsonElement>> Prices { get; set; } = new();
}
