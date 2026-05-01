using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using brewbase.server.Services;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TastingSessionsController : ControllerBase
{
    private readonly ITastingSessionWriteService _tastingSessionWriteService;
	private readonly ITastingSessionReadService _tastingSessionReadService;
	private readonly ICurrentUserProvider _currentUserProvider;

    public TastingSessionsController(
    	ITastingSessionWriteService tastingSessionWriteService,
    	ITastingSessionReadService tastingSessionReadService,
    	ICurrentUserProvider currentUserProvider)

	{
    	_tastingSessionWriteService = tastingSessionWriteService;
    	_tastingSessionReadService = tastingSessionReadService;
    	_currentUserProvider = currentUserProvider;
	}

    [HttpPost]
    [ProducesResponseType(typeof(TastingSessionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTastingSessionRequestDto request)
    {
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
    	var userId = _currentUserProvider.GetUserId();

    	if (userId is null)
    	{
        	return Unauthorized();
    	}

    	var tastingSessions = await _tastingSessionReadService.GetUserSessionsAsync(userId.Value);

    	return Ok(tastingSessions);
	}

    [HttpGet("{id:int}")]
	[ProducesResponseType(typeof(TastingSessionDetailsResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetById(int id)
	{
    	var userId = _currentUserProvider.GetUserId();

    	if (userId is null)
    	{
        	return Unauthorized();
    	}

    	var tastingSession = await _tastingSessionReadService.GetSessionDetailsAsync(id, userId.Value);

    	if (tastingSession is null)
    	{
        	return NotFound();
    	}	

    	return Ok(tastingSession);
	}
}