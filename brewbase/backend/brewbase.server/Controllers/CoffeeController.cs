using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeController : ControllerBase
{
    private readonly ICoffeeReadService _coffeeReadService;

    public CoffeeController(ICoffeeReadService coffeeReadService)
    {
        _coffeeReadService = coffeeReadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? regionId,
        [FromQuery] int? roasteryId,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var coffees = await _coffeeReadService.GetAllAsync(
            regionId,
            roasteryId,
            search,
            sortBy,
            sortOrder,
            page,
            pageSize);

        return Ok(coffees);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var coffee = await _coffeeReadService.GetByIdAsync(id);

        if (coffee == null)
        {
            return NotFound();
        }

        return Ok(coffee);
    }
    
}