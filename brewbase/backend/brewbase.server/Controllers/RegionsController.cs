using brewbase.server.Services.Interfaces;
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
}
