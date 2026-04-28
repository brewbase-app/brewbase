using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TastingSessionsController : ControllerBase
{
    private readonly ITastingSessionWriteService _tastingSessionWriteService;

    public TastingSessionsController(ITastingSessionWriteService tastingSessionWriteService)
    {
        _tastingSessionWriteService = tastingSessionWriteService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TastingSessionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTastingSessionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        var tastingSession = await _tastingSessionWriteService.CreateAsync(request);

        if (tastingSession is null)
        {
            return Unauthorized();
        }

        return Created($"/api/TastingSessions/{tastingSession.Id}", tastingSession);
    }
}