namespace brewbase.server.Dtos;

public class CoffeeRankingResponseDto
{
    public int Position { get; set; }

    public int CoffeeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public string? Roastery { get; set; }

    public string? ProcessingMethod { get; set; }

    public string? Variety { get; set; }

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }

    public int RecipeUsedCount { get; set; }
}