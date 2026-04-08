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
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
		var query = _context.Recipes
        	.OrderBy(r => r.Id)
        	.AsQueryable();

   	 	if (page.HasValue && pageSize.HasValue)
    	{
        	var skip = (page.Value - 1) * pageSize.Value;
        	query = query.Skip(skip).Take(pageSize.Value);
    	}
        var recipes = await query
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