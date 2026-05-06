using System.Text.Json;
using CryptoDashboardAPI.DTOs.External;
using CryptoDashboardAPI.Exceptions;

namespace CryptoDashboardAPI.Services;

public class CoinGeckoClient : ICoinGeckoClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinGeckoClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CoinGeckoClient(HttpClient httpClient, IConfiguration configuration, ILogger<CoinGeckoClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var apiKey = configuration["CoinGecko:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            _httpClient.DefaultRequestHeaders.Add("x-cg-demo-api-key", apiKey);
    }

    public async Task<List<CoinGeckoMarketDto>> GetMarketsAsync(int perPage = 100, int page = 1)
    {
        var url = $"coins/markets?vs_currency=usd&order=market_cap_desc&per_page={perPage}&page={page}&sparkline=false&price_change_percentage=24h";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CoinGecko markets request failed with status {Status}", response.StatusCode);
                throw new ExternalApiException($"CoinGecko returned {(int)response.StatusCode}.");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CoinGeckoMarketDto>>(content, JsonOptions) ?? new List<CoinGeckoMarketDto>();
        }
        catch (ExternalApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch markets from CoinGecko");
            throw new ExternalApiException("Failed to reach CoinGecko API.", ex);
        }
    }

    public async Task<CoinGeckoChartDto> GetMarketChartAsync(string externalProviderId, int days)
    {
        var url = $"coins/{Uri.EscapeDataString(externalProviderId)}/market_chart?vs_currency=usd&days={days}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CoinGecko chart request failed for {Coin} with status {Status}", externalProviderId, response.StatusCode);
                throw new ExternalApiException($"CoinGecko returned {(int)response.StatusCode} for coin '{externalProviderId}'.");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CoinGeckoChartDto>(content, JsonOptions) ?? new CoinGeckoChartDto();
        }
        catch (ExternalApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch market chart from CoinGecko for {Coin}", externalProviderId);
            throw new ExternalApiException("Failed to reach CoinGecko API.", ex);
        }
    }
}
