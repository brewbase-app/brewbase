using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeController : ControllerBase
{
    private readonly ICoffeeReadService _coffeeReadService;
    private readonly ICoffeeFavoriteService _coffeeFavoriteService;
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CoffeeController(
        ICoffeeReadService coffeeReadService,
        ICoffeeFavoriteService coffeeFavoriteService,
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _coffeeReadService = coffeeReadService;
        _coffeeFavoriteService = coffeeFavoriteService;
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
        var currentUserId = _currentUserProvider.GetUserId();

        var coffees = await _coffeeReadService.GetAllAsync(
            regionId,
            roasteryId,
            search,
            sortBy,
            sortOrder,
            page,
            pageSize,
            currentUserId);

        return Ok(coffees);
    }

    [Authorize]
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(List<CoffeeListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites()
    {
        var favorites = await _coffeeFavoriteService.GetMyFavoritesAsync();

        if (favorites is null)
        {
            return Unauthorized();
        }

        return Ok(favorites);
    }

    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<CoffeeLookupResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LookupByName([FromQuery] string name, [FromQuery] int limit = 10)
    {
        var matches = await _coffeeReadService.LookupByNameAsync(name, limit);
        return Ok(matches);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = _currentUserProvider.GetUserId();
        var coffee = await _coffeeReadService.GetByIdAsync(id, currentUserId);

        if (coffee == null)
        {
            return NotFound();
        }

        return Ok(coffee);
    }

    [Authorize]
    [HttpPost("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var result = await _coffeeFavoriteService.AddAsync(id);

        return result switch
        {
            FavoriteServiceStatus.Unauthorized => Unauthorized(),
            FavoriteServiceStatus.NotFound => NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." }),
            _ => NoContent()
        };
    }

    [Authorize]
    [HttpDelete("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var result = await _coffeeFavoriteService.RemoveAsync(id);

        if (result == FavoriteServiceStatus.Unauthorized)
        {
            return Unauthorized();
        }

        return NoContent();
    }
    
    [Authorize]
    [HttpPost("{id:int}/rating")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateCoffee(int id, [FromBody] RateRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }
        
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var coffee = await _context.Coffees.FirstOrDefaultAsync(c => c.Id == id);

        if (coffee is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." });
        }

        if (coffee.CreatedByUserId == userId.Value)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new SimpleErrorResponseDto { Message = "You cannot rate your own coffee." });
        }

        var rating = await _context.CoffeeRatings
            .FirstOrDefaultAsync(r => r.CoffeeId == id && r.UserId == userId.Value);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var isNewRating = rating is null;

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

        if (isNewRating)
        {
            var user = await _context.AppUsers.FindAsync(userId.Value);

            if (user != null)
            {
                user.ActivityPoints += 10;
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}
