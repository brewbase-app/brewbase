using System;
using System.Collections.Generic;
using DefaultNamespace;

namespace brewbase.server.Models;

public partial class UserPreference
{
    public int Id { get; set; }

    public string PreferredRoastLevel { get; set; } = null!;

    public string FavoriteNotes { get; set; } = null!;

    public bool QuizCompleted { get; set; }

    public int UserId { get; set; }
    
    public string? ExperienceLevel { get; set; }

    public string? PreferredAcidity { get; set; }

    public string? PreferredBody { get; set; }

    public string? RecommendationStyle { get; set; }

    public bool AllowExploration { get; set; }

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual AppUser User { get; set; } = null!;
    
    public virtual ICollection<UserPreferenceFlavorProfile> UserPreferenceFlavorProfiles { get; set; } = new List<UserPreferenceFlavorProfile>();

    public virtual ICollection<UserPreferenceRegion> UserPreferenceRegions { get; set; } = new List<UserPreferenceRegion>();

    public virtual ICollection<UserPreferenceBrewingMethod> UserPreferenceBrewingMethods { get; set; } = new List<UserPreferenceBrewingMethod>();
}
