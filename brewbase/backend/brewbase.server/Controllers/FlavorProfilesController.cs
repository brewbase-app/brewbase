using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/flavor-profiles")]
public class FlavorProfilesController : ControllerBase
{
    private readonly IFlavorProfileService _flavorProfileService;

    public FlavorProfilesController(IFlavorProfileService flavorProfileService)
    {
        _flavorProfileService = flavorProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _flavorProfileService.GetAllAsync();
        return Ok(profiles);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 20)
    {
        var profiles = await _flavorProfileService.SearchAsync(q, limit);
        return Ok(profiles);
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandom([FromQuery] int limit = 10)
    {
        var profiles = await _flavorProfileService.GetRandomAsync(limit);
        return Ok(profiles);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateFlavorProfileRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var profile = await _flavorProfileService.CreateAsync(dto);
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new SimpleErrorResponseDto { Message = ex.Message });
        }
    }
}
