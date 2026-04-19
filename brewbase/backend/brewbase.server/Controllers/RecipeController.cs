using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeReadService _recipeReadService;

    public RecipeController(IRecipeReadService recipeReadService)
    {
        _recipeReadService = recipeReadService;
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
        var recipes = await _recipeReadService.GetAllAsync(
            coffeeId,
            userId,
            brewingMethodId,
            search,
            sortBy,
            sortOrder,
            page,
            pageSize);

        return Ok(recipes);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var recipe = await _recipeReadService.GetByIdAsync(id);

        if (recipe == null)
        {
            return NotFound();
        }

        return Ok(recipe);
    }
}