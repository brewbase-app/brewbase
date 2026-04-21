using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Controllers;

/// <summary>
/// Write operations for recipes (delete). Kept in a partial file to reduce merge overlap with read-layer work on <see cref="RecipeController"/>.
/// </summary>
public partial class RecipeController
{
    /// <summary>
    /// Deletes a recipe. Only the owner may delete it.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            return NotFound();
        }

        if (recipe.UserId != userId.Value)
        {
            return Forbid();
        }

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
