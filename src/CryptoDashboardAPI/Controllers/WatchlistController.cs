using System.Security.Claims;
using CryptoDashboardAPI.DTOs.Watchlist;
using CryptoDashboardAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoDashboardAPI.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly WatchlistService _watchlistService;

    public WatchlistController(WatchlistService watchlistService) => _watchlistService = watchlistService;

    /// <summary>Add a cryptocurrency to the authenticated user's watchlist.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddWatchlistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] AddWatchlistRequest request)
    {
        var result = await _watchlistService.AddAsync(GetUserId(), request);
        return CreatedAtAction(nameof(Add), result);
    }

    /// <summary>Get the authenticated user's watchlist with current prices.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(WatchlistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get() =>
        Ok(await _watchlistService.GetAsync(GetUserId()));

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User ID claim is missing.");
        return Guid.Parse(sub);
    }
}
