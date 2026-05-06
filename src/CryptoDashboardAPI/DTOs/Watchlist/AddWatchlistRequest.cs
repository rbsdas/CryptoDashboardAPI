using System.ComponentModel.DataAnnotations;

namespace CryptoDashboardAPI.DTOs.Watchlist;

public class AddWatchlistRequest
{
    [Required]
    public Guid CryptocurrencyId { get; set; }
}
