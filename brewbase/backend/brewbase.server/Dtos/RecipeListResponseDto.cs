namespace brewbase.server.Dtos;

public class RecipeListResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Parameters { get; set; } = null!;

    public string Steps { get; set; } = null!;

    public bool IsPublic { get; set; }

    public int UserId { get; set; }

    public int? CoffeeId { get; set; }

    public int? BrewingMethodId { get; set; }

    public string? BrewingMethod { get; set; }

    public string? Coffee { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsFavorite { get; set; }
}
