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

    public RankingController(
        IRankingReadService rankingReadService,
        IRankingRefreshService rankingRefreshService)
    {
        _rankingReadService = rankingReadService;
        _rankingRefreshService = rankingRefreshService;
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshRankings(CancellationToken cancellationToken)
    {
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
}
