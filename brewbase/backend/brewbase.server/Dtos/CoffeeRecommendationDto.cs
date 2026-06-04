namespace DefaultNamespace;

public class CoffeeRecommendationDto
{
    public int CoffeeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Region { get; set; }

    public string? ProcessingMethod { get; set; }

    public string? Variety { get; set; }

    public string? Roastery { get; set; }

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }

    public double MatchScore { get; set; }

    public double PopularityScore { get; set; }

    public double FinalScore { get; set; }
}