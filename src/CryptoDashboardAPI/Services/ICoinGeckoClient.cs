using CryptoDashboardAPI.DTOs.External;

namespace CryptoDashboardAPI.Services;

public interface ICoinGeckoClient
{
    Task<List<CoinGeckoMarketDto>> GetMarketsAsync(int perPage = 100, int page = 1);
    Task<CoinGeckoChartDto> GetMarketChartAsync(string externalProviderId, int days);
}
