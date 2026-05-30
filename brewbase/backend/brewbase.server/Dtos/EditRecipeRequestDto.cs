using System.Text.Json;

namespace brewbase.server.Dtos;

/// <summary>
/// Request body for updating a recipe. User id is resolved server-side; ownership is enforced.
/// </summary>
public sealed class EditRecipeRequestDto
{
    public string? Title { get; set; }

    /// <summary>
    /// JSON value stored in <c>recipe.parameters</c> (jsonb).
    /// </summary>
    public JsonElement Parameters { get; set; }

    public string? Steps { get; set; }

    public bool IsPublic { get; set; }

    public int? CoffeeId { get; set; }

    public int? BrewingMethodId { get; set; }
}
