namespace DefaultNamespace;

public class CoffeeRecommendationDto
{
    public int CoffeeId { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public double AverageRating { get; set; }
    
    public double MatchScore { get; set; }

    public double PopularityScore { get; set; }

    public double FinalScore { get; set; }
}