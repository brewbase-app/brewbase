using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrewingMethodsController : ControllerBase
{
    private readonly IBrewingMethodReadService _brewingMethodReadService;

    public BrewingMethodsController(IBrewingMethodReadService brewingMethodReadService)
    {
        _brewingMethodReadService = brewingMethodReadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brewingMethods = await _brewingMethodReadService.GetAllAsync();
        return Ok(brewingMethods);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brewingMethod = await _brewingMethodReadService.GetByIdAsync(id);

        if (brewingMethod == null)
        {
            return NotFound();
        }

        return Ok(brewingMethod);
    }
}