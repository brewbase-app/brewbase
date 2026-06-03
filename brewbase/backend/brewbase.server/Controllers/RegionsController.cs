using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/regions")]
public class RegionsController : ControllerBase
{
    private readonly IRegionService _regionService;

    public RegionsController(IRegionService regionService)
    {
        _regionService = regionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? countryId)
    {
        var regions = await _regionService.GetAllAsync(countryId);
        return Ok(regions);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] int? countryId,
        [FromQuery] string? q,
        [FromQuery] int limit = 20)
    {
        if (!countryId.HasValue)
        {
            return BadRequest(new SimpleErrorResponseDto
            {
                Message = "Parametr countryId jest wymagany."
            });
        }

        try
        {
            var regions = await _regionService.SearchAsync(
                countryId.Value,
                q,
                limit);

            return Ok(regions);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new SimpleErrorResponseDto { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateRegionRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var region = await _regionService.CreateAsync(dto);
            return Ok(region);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new SimpleErrorResponseDto { Message = ex.Message });
        }
    }
}
