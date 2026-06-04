using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/roasteries")]
public class RoasteriesController : ControllerBase
{
    private readonly IRoasteryService _roasteryService;

    public RoasteriesController(IRoasteryService roasteryService)
    {
        _roasteryService = roasteryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roasteries = await _roasteryService.GetAllAsync();
        return Ok(roasteries);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 20)
    {
        var roasteries = await _roasteryService.SearchAsync(q, limit);
        return Ok(roasteries);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateRoasteryRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var roastery = await _roasteryService.CreateAsync(dto);
            return Ok(roastery);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new SimpleErrorResponseDto { Message = ex.Message });
        }
    }
}
