namespace DefaultNamespace;

public class RecipeRecommendationDto
{
    public int RecipeId { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public double AverageRating { get; set; }
    public double MatchScore { get; set; }

    public double PopularityScore { get; set; }

    public double FinalScore { get; set; }
}