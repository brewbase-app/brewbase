namespace brewbase.server.Dtos;

public class SaveUserPreferencesRequestDto
{
    public string ExperienceLevel { get; set; } = string.Empty;

    public string PreferredRoastLevel { get; set; }
    public List<string> FlavorProfiles { get; set; } = [];
    public List<string> BrewingMethods { get; set; } = [];
    public List<string> Regions { get; set; } = [];

    public string PreferredAcidity { get; set; } = string.Empty;

    public string PreferredBody { get; set; } = string.Empty;

    public string RecommendationStyle { get; set; } = string.Empty;

    public bool AllowExploration { get; set; }
}