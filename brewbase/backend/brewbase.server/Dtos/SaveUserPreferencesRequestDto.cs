namespace brewbase.server.Dtos;

public class SaveUserPreferencesRequestDto
{
    public string? ExperienceLevel { get; set; }

    public string? PreferredRoastLevel { get; set; }

    public string? PreferredAcidity { get; set; }

    public string? PreferredBody { get; set; }

    public string? RecommendationStyle { get; set; }

    public bool AllowExploration { get; set; }

    public List<int> FlavorProfileIds { get; set; } = [];

    public List<int> BrewingMethodIds { get; set; } = [];

    public List<int> RegionIds { get; set; } = [];
}