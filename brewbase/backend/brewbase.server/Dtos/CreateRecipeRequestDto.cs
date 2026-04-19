using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace brewbase.server.Dtos;

/// <summary>
/// Request body for creating a recipe. User id is resolved server-side (current user), not from the client.
/// </summary>
public sealed class CreateRecipeRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// JSON value stored in <c>recipe.parameters</c> (jsonb).
    /// </summary>
    public JsonElement Parameters { get; set; }

    [Required]
    public string Steps { get; set; } = null!;

    public bool IsPublic { get; set; }

    [Range(1, int.MaxValue)]
    public int CoffeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int BrewingMethodId { get; set; }
}
