namespace brewbase.server.Dtos;

public class RecommendationSummaryFeedbackRequestDto
{
    public int Rating { get; set; }

    public string PreferenceAction { get; set; } = string.Empty;
}