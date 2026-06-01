using System.Text.Json;
using brewbase.server.Services.Interfaces;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeReadService _recipeReadService;
    private readonly IRecipeFavoriteService _recipeFavoriteService;
    private readonly IRecipeValidationService _recipeValidationService;
    private readonly IRankingRefreshService _rankingRefreshService;
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public RecipeController(
        IRecipeReadService recipeReadService,
        IRecipeFavoriteService recipeFavoriteService,
        IRecipeValidationService recipeValidationService,
        IRankingRefreshService rankingRefreshService,
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _recipeReadService = recipeReadService;
        _recipeFavoriteService = recipeFavoriteService;
        _recipeValidationService = recipeValidationService;
        _rankingRefreshService = rankingRefreshService;
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    /// <summary>Returns recipes visible to the current user.</summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<RecipeListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        var currentUserId = _currentUserProvider.GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var recipes = await _recipeReadService.GetAllAsync(
            coffeeId,
            userId,
            brewingMethodId,
            search,
            sortBy,
            sortOrder,
            page,
            pageSize,
            currentUserId.Value);

        return Ok(recipes);
    }

    [Authorize]
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(List<RecipeListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites()
    {
        var favorites = await _recipeFavoriteService.GetMyFavoritesAsync();

        if (favorites is null)
        {
            return Unauthorized();
        }

        return Ok(favorites);
    }
    
    /// <summary>Returns a recipe if visible to the current user; otherwise 404.</summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RecipeDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = _currentUserProvider.GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var recipe = await _recipeReadService.GetByIdAsync(id, currentUserId.Value);

        if (recipe == null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        return Ok(recipe);
    }

    [Authorize]
    [HttpPost("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var result = await _recipeFavoriteService.AddAsync(id);

        return result switch
        {
            FavoriteServiceStatus.Unauthorized => Unauthorized(),
            FavoriteServiceStatus.NotFound => NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." }),
            _ => NoContent()
        };
    }

    [Authorize]
    [HttpDelete("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var result = await _recipeFavoriteService.RemoveAsync(id);

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
    public async Task<IActionResult> RateRecipe(int id, [FromBody] RateRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var recipe = await RecipeReadService.WhereVisibleTo(_context.Recipes, userId.Value)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        if (recipe.UserId == userId.Value)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new SimpleErrorResponseDto { Message = "You cannot rate your own recipe." });
        }

        var rating = await _context.RecipeRatings
            .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId.Value);

        var now = DateTime.UtcNow;

        if (rating is null)
        {
            rating = new RecipeRating
            {
                RecipeId = id,
                UserId = userId.Value,
                Value = request.Value,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.RecipeRatings.Add(rating);
        }
        else
        {
            rating.Value = request.Value;
            rating.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Creates a recipe for the current user. User id comes from context, not the body.</summary>
    [Authorize]
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

        var validationError = ApplyValidation(request.IsPublic, request.Title, request.Steps, request.Parameters, request.CoffeeId, request.BrewingMethodId);
        if (validationError is not null)
        {
            return validationError;
        }

        var foreignKeyError = await ValidateForeignKeysAsync(request.CoffeeId, request.BrewingMethodId);
        if (foreignKeyError is not null)
        {
            return foreignKeyError;
        }

        var entity = new Recipe
        {
            Title = request.Title?.Trim() ?? string.Empty,
            Parameters = NormalizeParameters(request.Parameters),
            Steps = request.Steps?.Trim() ?? string.Empty,
            IsPublic = request.IsPublic,
            UserId = userId.Value,
            CoffeeId = NormalizeForeignKey(request.CoffeeId),
            BrewingMethodId = NormalizeForeignKey(request.BrewingMethodId),
            CreatedAt = DateTime.UtcNow
        };

        _context.Recipes.Add(entity);
        await _context.SaveChangesAsync();
        await _rankingRefreshService.RefreshAllRankingsAsync();

        var detail = await _recipeReadService.GetByIdAsync(entity.Id, userId.Value);
        if (detail is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, detail);
    }

    /// <summary>Owner only. 404 if not visible to the caller; 403 if visible but not owned.</summary>
    [Authorize]
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

        var validationError = ApplyValidation(request.IsPublic, request.Title, request.Steps, request.Parameters, request.CoffeeId, request.BrewingMethodId);
        if (validationError is not null)
        {
            return validationError;
        }

        var recipe = await RecipeReadService.WhereVisibleTo(_context.Recipes, userId.Value)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        if (recipe.UserId != userId.Value)
        {
            return Forbid();
        }

        var foreignKeyError = await ValidateForeignKeysAsync(request.CoffeeId, request.BrewingMethodId);
        if (foreignKeyError is not null)
        {
            return foreignKeyError;
        }

        recipe.Title = request.Title?.Trim() ?? string.Empty;
        recipe.Parameters = NormalizeParameters(request.Parameters);
        recipe.Steps = request.Steps?.Trim() ?? string.Empty;
        recipe.IsPublic = request.IsPublic;
        recipe.CoffeeId = NormalizeForeignKey(request.CoffeeId);
        recipe.BrewingMethodId = NormalizeForeignKey(request.BrewingMethodId);

        if (request.IsPublic)
        {
            recipe.ModerationComment = null;
        }

        await _context.SaveChangesAsync();
        await _rankingRefreshService.RefreshAllRankingsAsync();

        var detail = await _recipeReadService.GetByIdAsync(id, userId.Value);
        if (detail is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        return Ok(detail);
    }

    /// <summary>Owner only. 404 if not visible; 403 if visible but not owned.</summary>
    [Authorize]
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

        var recipe = await RecipeReadService.WhereVisibleTo(_context.Recipes, userId.Value)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Recipe not found." });
        }

        if (recipe.UserId != userId.Value)
        {
            return Forbid();
        }

        await _context.RecipeRatings
            .Where(r => r.RecipeId == id)
            .ExecuteDeleteAsync();

        await _context.UserRecipeFavorites
            .Where(f => f.RecipeId == id)
            .ExecuteDeleteAsync();

        await _context.RecipeRankings
            .Where(r => r.RecipeId == id)
            .ExecuteDeleteAsync();

        await _context.Recommendations
            .Where(r => r.RecipeId == id)
            .ExecuteDeleteAsync();

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        await _rankingRefreshService.RefreshAllRankingsAsync();

        return NoContent();
    }

    private IActionResult? ApplyValidation(
        bool isPublic,
        string? title,
        string? steps,
        JsonElement parameters,
        int? coffeeId,
        int? brewingMethodId)
    {
        var validationResult = isPublic
            ? _recipeValidationService.ValidateForPublish(title, steps, parameters, coffeeId, brewingMethodId)
            : _recipeValidationService.ValidateDraft(title, steps, parameters, coffeeId, brewingMethodId);

        if (validationResult.IsValid)
        {
            return null;
        }

        foreach (var (field, messages) in validationResult.Errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError(field, message);
            }
        }

        return ValidationProblem(ModelState);
    }

    private async Task<IActionResult?> ValidateForeignKeysAsync(int? coffeeId, int? brewingMethodId)
    {
        if (coffeeId is > 0 && !await _context.Coffees.AnyAsync(c => c.Id == coffeeId.Value))
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Coffee not found." });
        }

        if (brewingMethodId is > 0 && !await _context.BrewingMethods.AnyAsync(b => b.Id == brewingMethodId.Value))
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Brewing method not found." });
        }

        return null;
    }

    private static int? NormalizeForeignKey(int? foreignKey) =>
        foreignKey is > 0 ? foreignKey : null;

    private static string NormalizeParameters(JsonElement parameters)
    {
        if (parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return "{}";
        }

        return parameters.GetRawText();
    }
}
