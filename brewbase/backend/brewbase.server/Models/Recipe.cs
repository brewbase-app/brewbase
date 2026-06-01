using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace brewbase.server.Models;

public partial class Recipe
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Parameters { get; set; } = null!;

    public string Steps { get; set; } = null!;

    public bool IsPublic { get; set; }

    [NotMapped]
    public RecipeStatus Status
    {
        get => RecipeStatusExtensions.FromIsPublic(IsPublic);
        set => IsPublic = value.ToIsPublic();
    }

    public int UserId { get; set; }

    public int? BrewingMethodId { get; set; }

    public int? CoffeeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ModerationComment { get; set; }

    public virtual BrewingMethod? BrewingMethod { get; set; }

    public virtual Coffee? Coffee { get; set; }

    public virtual ICollection<RecipeRanking> RecipeRankings { get; set; } = new List<RecipeRanking>();

    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual AppUser User { get; set; } = null!;

    public virtual ICollection<UserRecipeFavorite> UserRecipeFavorites { get; set; } = new List<UserRecipeFavorite>();
}
