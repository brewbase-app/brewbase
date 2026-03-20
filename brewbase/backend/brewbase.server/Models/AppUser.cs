using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int ActivityPoints { get; set; }

    public string? Label { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Article> ArticleModeratedByUsers { get; set; } = new List<Article>();

    public virtual ICollection<Article> ArticleUsers { get; set; } = new List<Article>();

    public virtual ICollection<CoffeeRating> CoffeeRatings { get; set; } = new List<CoffeeRating>();

    public virtual ICollection<Coffee> Coffees { get; set; } = new List<Coffee>();

    public virtual ICollection<CuppingSession> CuppingSessions { get; set; } = new List<CuppingSession>();

    public virtual ICollection<Follow> FollowFolloweds { get; set; } = new List<Follow>();

    public virtual ICollection<Follow> FollowFollowers { get; set; } = new List<Follow>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<QuickNote> QuickNotes { get; set; } = new List<QuickNote>();

    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();

    public virtual ICollection<UserRanking> UserRankings { get; set; } = new List<UserRanking>();
}
