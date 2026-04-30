using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TastingSessionsController : ControllerBase
{
    private readonly ITastingSessionWriteService _tastingSessionWriteService;
	private readonly ITastingSessionReadService _tastingSessionReadService;

    public TastingSessionsController(
    	ITastingSessionWriteService tastingSessionWriteService,
    	ITastingSessionReadService tastingSessionReadService)
	{
    	_tastingSessionWriteService = tastingSessionWriteService;
    	_tastingSessionReadService = tastingSessionReadService;
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
	
	[HttpGet]
    [ProducesResponseType(typeof(List<TastingSessionListItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var tastingSessions = await _tastingSessionReadService.GetUserSessionsAsync();

        if (tastingSessions is null)
        {
            return Unauthorized();
        }

        return Ok(tastingSessions);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TastingSessionDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var tastingSession = await _tastingSessionReadService.GetSessionDetailsAsync(id);

        if (tastingSession is null)
        {
            return NotFound();
        }

        return Ok(tastingSession);
    }
}