using System.Text.Json;
using brewbase.server.Services.Interfaces;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeReadService _recipeReadService;
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RecipeController(
        IRecipeReadService recipeReadService,
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _recipeReadService = recipeReadService;
        _context = context;
        _currentUserProvider = currentUserProvider;
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
    [ProducesResponseType(typeof(RecipeDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var recipe = await _recipeReadService.GetByIdAsync(id);

        if (recipe == null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        return Ok(recipe);
    }

    /// <summary>
    /// Creates a recipe for the current user. User id is never taken from the request body.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeDetailResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!ValidateRecipeRequest(request.Title, request.Steps, request.Parameters))
        {
            return ValidationProblem(ModelState);
        }

        var coffeeExists = await _context.Coffees.AnyAsync(c => c.Id == request.CoffeeId);
        if (!coffeeExists)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." });
        }

        var brewingMethodExists = await _context.BrewingMethods.AnyAsync(b => b.Id == request.BrewingMethodId);
        if (!brewingMethodExists)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Brewing method not found." });
        }

        var entity = new Recipe
        {
            Title = request.Title.Trim(),
            Parameters = request.Parameters.GetRawText(),
            Steps = request.Steps.Trim(),
            IsPublic = request.IsPublic,
            UserId = userId.Value,
            CoffeeId = request.CoffeeId,
            BrewingMethodId = request.BrewingMethodId
        };

        _context.Recipes.Add(entity);
        await _context.SaveChangesAsync();

        var detailEntity = await _context.Recipes
            .Include(r => r.BrewingMethod)
            .Include(r => r.Coffee)
            .Where(r => r.Id == entity.Id)
            .FirstAsync();
        var detail = MapToRecipeDetailResponseDto(detailEntity);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, detail);
    }

    /// <summary>
    /// Updates a recipe. Only the owner may edit; coffee and brewing method must exist.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RecipeDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EditRecipeRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!ValidateRecipeRequest(request.Title, request.Steps, request.Parameters))
        {
            return ValidationProblem(ModelState);
        }

        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        if (recipe.UserId != userId.Value)
        {
            return Forbid();
        }

        var coffeeExists = await _context.Coffees.AnyAsync(c => c.Id == request.CoffeeId);
        if (!coffeeExists)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." });
        }

        var brewingMethodExists = await _context.BrewingMethods.AnyAsync(b => b.Id == request.BrewingMethodId);
        if (!brewingMethodExists)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Brewing method not found." });
        }

        recipe.Title = request.Title.Trim();
        recipe.Parameters = request.Parameters.GetRawText();
        recipe.Steps = request.Steps.Trim();
        recipe.IsPublic = request.IsPublic;
        recipe.CoffeeId = request.CoffeeId;
        recipe.BrewingMethodId = request.BrewingMethodId;

        await _context.SaveChangesAsync();

        var detailEntity = await _context.Recipes
            .Include(r => r.BrewingMethod)
            .Include(r => r.Coffee)
            .Where(r => r.Id == id)
            .FirstAsync();
        var detail = MapToRecipeDetailResponseDto(detailEntity);

        return Ok(detail);
    }

    /// <summary>
    /// Deletes a recipe. Only the owner may delete it.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        if (recipe.UserId != userId.Value)
        {
            return Forbid();
        }

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ValidateRecipeRequest(string? title, string? steps, JsonElement parameters)
    {
        if (parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            ModelState.AddModelError("Parameters", "Parameters must be a JSON value.");
        }

        if (string.IsNullOrWhiteSpace(steps))
        {
            ModelState.AddModelError("Steps", "Steps are required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError("Title", "Title is required.");
        }

        return ModelState.IsValid;
    }

    private static RecipeDetailResponseDto MapToRecipeDetailResponseDto(Recipe recipe)
    {
        return new RecipeDetailResponseDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Parameters = recipe.Parameters,
            Steps = recipe.Steps,
            IsPublic = recipe.IsPublic,
            UserId = recipe.UserId,
            BrewingMethod = recipe.BrewingMethod?.Name,
            Coffee = recipe.Coffee?.Name
        };
    }
}
