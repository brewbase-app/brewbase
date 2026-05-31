using brewbase.server.Dtos;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IGlobalSearchService _globalSearchService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SearchController(
        IGlobalSearchService globalSearchService,
        ICurrentUserProvider currentUserProvider)
    {
        _globalSearchService = globalSearchService;
        _currentUserProvider = currentUserProvider;
    }

    /// <summary>
    /// Global search across coffees, recipes, users, wiki articles, quick notes and cupping sessions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GlobalSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _globalSearchService.SearchAsync(
            userId.Value,
            query,
            limit,
            cancellationToken);
        return Ok(response);
    }
}
