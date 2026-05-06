using CryptoDashboardAPI.DTOs.Crypto;
using CryptoDashboardAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoDashboardAPI.Controllers;

[ApiController]
[Route("api/cryptocurrencies")]
public class CryptocurrenciesController : ControllerBase
{
    private readonly CryptoService _cryptoService;

    public CryptocurrenciesController(CryptoService cryptoService) => _cryptoService = cryptoService;

    /// <summary>Get a paginated list of cryptocurrencies. Call /refresh first to populate data.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CryptoListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        return Ok(await _cryptoService.GetAllAsync(page, pageSize));
    }

    /// <summary>Get details for a single cryptocurrency by its internal ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CryptoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _cryptoService.GetByIdAsync(id));

    /// <summary>Refresh all cryptocurrency data from CoinGecko. Subject to a 60-second cooldown.</summary>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Refresh() =>
        Ok(await _cryptoService.RefreshAsync());

    /// <summary>Get historical price data for a cryptocurrency. Allowed days: 1, 7, 14, 30, 90, 180, 365.</summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(CryptoHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetHistory(Guid id, [FromQuery] int days = 7) =>
        Ok(await _cryptoService.GetHistoryAsync(id, days));
}
