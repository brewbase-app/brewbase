using System.Text.Json;

namespace brewbase.server.Dtos;

/// <summary>
/// Request body for creating a recipe. User id is resolved server-side (current user), not from the client.
/// </summary>
public sealed class CreateRecipeRequestDto
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
