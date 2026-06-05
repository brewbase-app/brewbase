namespace brewbase.server.Models;

public class RecommendationFeedbackSummary
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string PreferenceAction { get; set; } = string.Empty;

    public string? PreviousRecommendationStyle { get; set; }

    public string NewRecommendationStyle { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}