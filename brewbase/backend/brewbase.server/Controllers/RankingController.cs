using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly IRankingReadService _rankingReadService;
    private readonly IRankingRefreshService _rankingRefreshService;
    private readonly IConfiguration _configuration;

    public RankingController(
        IRankingReadService rankingReadService,
        IRankingRefreshService rankingRefreshService,
        IConfiguration configuration)
    {
        _rankingReadService = rankingReadService;
        _rankingRefreshService = rankingRefreshService;
        _configuration = configuration;
    }

    [HttpGet("coffees")]
    [ProducesResponseType(typeof(List<CoffeeRankingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoffeeRanking([FromQuery] int limit = 10)
    {
        var ranking = await _rankingReadService.GetCoffeeRankingAsync(limit);

        return Ok(ranking);
    }
    
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserRankingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRanking([FromQuery] int limit = 10)
    {
        var ranking = await _rankingReadService.GetUserRankingAsync(limit);

        return Ok(ranking);
    }
    
    [HttpGet("recipes")]
    [ProducesResponseType(typeof(List<RecipeRankingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipeRanking([FromQuery] int limit = 10)
    {
        var ranking = await _rankingReadService.GetRecipeRankingAsync(limit);

        return Ok(ranking);
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshRankings(
        [FromHeader(Name = "X-Ranking-Refresh-Secret")] string? secret,
        CancellationToken cancellationToken)
    {
        if (!IsRefreshAuthorized(secret))
        {
            return Unauthorized(new SimpleErrorResponseDto
            {
                Message = "Brak uprawnień do odświeżenia rankingów."
            });
        }

        var refreshed = await _rankingRefreshService.TryRefreshAllRankingsAsync(cancellationToken);

        if (!refreshed)
        {
            return StatusCode(
                StatusCodes.Status409Conflict,
                new SimpleErrorResponseDto
                {
                    Message = "Odświeżanie rankingów jest już w toku lub niedostępne w tym środowisku."
                });
        }

        return NoContent();
    }

    private bool IsRefreshAuthorized(string? secret)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var configuredSecret = _configuration["RankingRefresh:Secret"];

        return !string.IsNullOrWhiteSpace(configuredSecret) &&
               string.Equals(
                   secret,
                   configuredSecret,
                   StringComparison.Ordinal);
    }
}
