namespace CryptoDashboardAPI.DTOs.Crypto;

public class CryptoListResponse
{
    public IEnumerable<CryptoResponse> Data { get; set; } = Array.Empty<CryptoResponse>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
