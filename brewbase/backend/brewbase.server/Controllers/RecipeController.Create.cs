using System.Text.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

/// <summary>
/// Write operations for recipes (create). Kept in a partial file to reduce merge overlap with read-layer work on <see cref="RecipeController"/>.
/// </summary>
public partial class RecipeController
{
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

        var detail = await _context.Recipes
            .Where(r => r.Id == entity.Id)
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

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, detail);
    }
}
