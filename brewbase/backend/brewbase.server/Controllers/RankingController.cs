using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly IRankingReadService _rankingReadService;

    public RankingController(IRankingReadService rankingReadService)
    {
        _rankingReadService = rankingReadService;
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
    
}