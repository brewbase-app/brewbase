using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeController : ControllerBase
{
    private readonly ICoffeeReadService _coffeeReadService;
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CoffeeController(
        ICoffeeReadService coffeeReadService,
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _coffeeReadService = coffeeReadService;
        _context = context;
        _currentUserProvider = currentUserProvider;
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
    
    [Authorize]
    [HttpPost("{id:int}/rating")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateCoffee(int id, [FromBody] RateRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var coffeeExists = await _context.Coffees.AnyAsync(c => c.Id == id);

        if (!coffeeExists)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." });
        }

        var rating = await _context.CoffeeRatings
            .FirstOrDefaultAsync(r => r.CoffeeId == id && r.UserId == userId.Value);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        if (rating is null)
        {
            rating = new CoffeeRating
            {
                CoffeeId = id,
                UserId = userId.Value,
                Value = request.Value,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.CoffeeRatings.Add(rating);
        }
        else
        {
            rating.Value = request.Value;
            rating.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}