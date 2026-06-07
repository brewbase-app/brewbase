using System.Text.Json;

namespace brewbase.server.Dtos;

public sealed class EditRecipeRequestDto
{
    public string? Title { get; set; }

    public JsonElement Parameters { get; set; }

    public string? Steps { get; set; }

    public bool IsPublic { get; set; }

    public int? CoffeeId { get; set; }

    public int? BrewingMethodId { get; set; }
}
