using brewbase.server.Dtos;
using brewbase.server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly BrewDbContext _context;

    public RecipeController(BrewDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? coffeeId,
        [FromQuery] int? userId,
        [FromQuery] int? brewingMethodId,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
		var query = _context.Recipes
        	.AsQueryable();

        if (coffeeId.HasValue)
        {
            query = query.Where(r => r.CoffeeId == coffeeId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (brewingMethodId.HasValue)
        {
            query = query.Where(r => r.BrewingMethodId == brewingMethodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Title != null && EF.Functions.ILike(r.Title, $"%{search}%"));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var isTitleSort = string.Equals(sortBy, "title", StringComparison.OrdinalIgnoreCase);

        query = isTitleSort
            ? (isDesc ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title))
            : query.OrderBy(r => r.Id);

   	 	if (page.HasValue && pageSize.HasValue)
    	{
        	var skip = (page.Value - 1) * pageSize.Value;
        	query = query.Skip(skip).Take(pageSize.Value);
    	}
        var recipes = await query
            .Select(r => new RecipeListResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null
            })
            .ToListAsync();

        return Ok(recipes);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var recipe = await _context.Recipes
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Parameters,
                r.Steps,
                r.IsPublic,
                UserId = r.UserId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null
            })
            .FirstOrDefaultAsync();

        if (recipe == null)
        {
            return NotFound();
        }

        return Ok(recipe);
    }
}