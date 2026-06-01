using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using brewbase.server.Services;

namespace brewbase.server.Controllers;

[ApiController]
[Authorize]
[Route("api/CuppingSessions")]
public class CuppingSessionsController : ControllerBase
{
    private readonly ICuppingSessionWriteService _tastingSessionWriteService;
	private readonly ICuppingSessionReadService _tastingSessionReadService;
	private readonly ICurrentUserProvider _currentUserProvider;

    public CuppingSessionsController(
    	ICuppingSessionWriteService tastingSessionWriteService,
    	ICuppingSessionReadService tastingSessionReadService,
    	ICurrentUserProvider currentUserProvider)

	{
    	_tastingSessionWriteService = tastingSessionWriteService;
    	_tastingSessionReadService = tastingSessionReadService;
    	_currentUserProvider = currentUserProvider;
	}

    [HttpPost]
    [ProducesResponseType(typeof(CuppingSessionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCuppingSessionRequestDto request)
    {
        var tastingSession = await _tastingSessionWriteService.CreateAsync(request);

        if (tastingSession is null)
        {
            return Unauthorized();
        }

        return Created($"/api/CuppingSessions/{tastingSession.Id}", tastingSession);
    }
	
	[HttpGet]
	[ProducesResponseType(typeof(List<CuppingSessionListItemResponseDto>), StatusCodes.Status200OK)]
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
	[ProducesResponseType(typeof(CuppingSessionDetailsResponseDto), StatusCodes.Status200OK)]
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

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CuppingSessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCuppingSessionRequestDto request)
    {
        var result = await _tastingSessionWriteService.UpdateSessionAsync(id, request);

        return result.Status switch
        {
            CuppingSessionWriteStatus.Success => Ok(result.Data),
            CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
            CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tastingSessionWriteService.DeleteSessionAsync(id);

        return result switch
        {
            CuppingSessionWriteStatus.Success => NoContent(),
            CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
            CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
            _ => BadRequest()
        };
    }

	[HttpPost("{id:int}/coffees")]
    [ProducesResponseType(typeof(CuppingSessionCoffeeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCoffee(
        int id,
        [FromBody] AddCoffeeToCuppingSessionRequestDto request)
    {
        var result = await _tastingSessionWriteService.AddCoffeeAsync(id, request);

        return result.Status switch
        {
            CuppingSessionWriteStatus.Success => CreatedAtAction(nameof(GetById), new { id }, result.Data),
            CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
            CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
            CuppingSessionWriteStatus.CoffeeNotFound => NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." }),
            CuppingSessionWriteStatus.CoffeeAlreadyAdded => Conflict(new SimpleErrorResponseDto { Message = "Coffee is already added to this tasting session." }),
            _ => BadRequest()
        };
    }
	
	[HttpPut("{sessionId:int}/coffees/{sessionCoffeeId:int}")]
	[ProducesResponseType(typeof(CuppingSessionCoffeeResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateCoffee(
		int sessionId,
		int sessionCoffeeId,
		[FromBody] UpdateCuppingSessionCoffeeRequestDto request)
	{
		var result = await _tastingSessionWriteService.UpdateCoffeeAsync(sessionId, sessionCoffeeId, request);

		return result.Status switch
		{
			CuppingSessionWriteStatus.Success => Ok(result.Data),
			CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
			CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
			CuppingSessionWriteStatus.CoffeeNotInSession => NotFound(new SimpleErrorResponseDto { Message = "Coffee is not added to this tasting session." }),
			_ => BadRequest()
		};
	}

	[HttpPut("{sessionId:int}/coffees/{sessionCoffeeId:int}/note")]
	[ProducesResponseType(typeof(CuppingSessionCoffeeResponseDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateCoffeeNote(
    	int sessionId,
    	int sessionCoffeeId,
    	[FromBody] UpdateCuppingSessionCoffeeNoteRequestDto request)
	{
    	var result = await _tastingSessionWriteService.UpdateCoffeeNoteAsync(sessionId, sessionCoffeeId, request);

    	return result.Status switch
    	{
        	CuppingSessionWriteStatus.Success => Ok(result.Data),
        	CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
        	CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
        	CuppingSessionWriteStatus.CoffeeNotInSession => NotFound(new SimpleErrorResponseDto { Message = "Coffee is not added to this tasting session." }),
        	_ => BadRequest()
    	};
	}

    [HttpDelete("{sessionId:int}/coffees/{sessionCoffeeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoffee(int sessionId, int sessionCoffeeId)
    {
        var result = await _tastingSessionWriteService.DeleteCoffeeAsync(sessionId, sessionCoffeeId);

        return result switch
        {
            CuppingSessionWriteStatus.Success => NoContent(),
            CuppingSessionWriteStatus.Unauthorized => Unauthorized(),
            CuppingSessionWriteStatus.CuppingSessionNotFound => NotFound(new SimpleErrorResponseDto { Message = "Tasting session not found." }),
            CuppingSessionWriteStatus.CoffeeNotInSession => NotFound(new SimpleErrorResponseDto { Message = "Coffee is not added to this tasting session." }),
            _ => BadRequest()
        };
    }
}
