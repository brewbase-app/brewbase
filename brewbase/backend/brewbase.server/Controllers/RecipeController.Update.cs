using System.Text.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

/// <summary>
/// Write operations for recipes (update). Kept in a partial file to reduce merge overlap with read-layer work on <see cref="RecipeController"/>.
/// </summary>
public partial class RecipeController
{
    /// <summary>
    /// Updates a recipe. Only the owner may edit; coffee and brewing method must exist.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RecipeDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EditRecipeRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (request.Parameters.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            ModelState.AddModelError(nameof(request.Parameters), "Parameters must be a JSON value.");
        }

        if (string.IsNullOrWhiteSpace(request.Steps))
        {
            ModelState.AddModelError(nameof(request.Steps), "Steps are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null)
        {
            return NotFound();
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

        var detail = await _context.Recipes
            .Where(r => r.Id == id)
            .Select(r => new RecipeDetailResponseDto
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
            .FirstAsync();

        return Ok(detail);
    }
}
