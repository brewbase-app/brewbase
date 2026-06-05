namespace DefaultNamespace;

public class UserPreferencesDto
{
    public string? ExperienceLevel { get; set; }

    public string? PreferredAcidity { get; set; }

    public string? PreferredBody { get; set; }

    public string? RecommendationStyle { get; set; }
    
    public List<string> FlavorProfiles { get; set; } = [];

    public List<string> BrewingMethods { get; set; } = [];

    public List<string> Regions { get; set; } = [];

}