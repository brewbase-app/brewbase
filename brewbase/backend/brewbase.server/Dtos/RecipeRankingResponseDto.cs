namespace brewbase.server.Dtos;

public class RecipeRankingResponseDto
{
    public int Position { get; set; }

    public int RecipeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Coffee { get; set; }

    public string? BrewingMethod { get; set; }

    public int UserId { get; set; }

    public string? UserLogin { get; set; }

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }

    public int SaveCount { get; set; }
}