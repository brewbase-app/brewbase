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
    public async Task<IActionResult> GetAll()
    {
        var recipes = await _context.Recipes
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
}