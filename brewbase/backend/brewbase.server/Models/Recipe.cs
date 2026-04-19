using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Recipe
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Parameters { get; set; } = null!;

    public string Steps { get; set; } = null!;

    public bool IsPublic { get; set; }

    public int UserId { get; set; }

    public int BrewingMethodId { get; set; }

    public int CoffeeId { get; set; }

    public virtual BrewingMethod BrewingMethod { get; set; } = null!;

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual ICollection<RecipeRanking> RecipeRankings { get; set; } = new List<RecipeRanking>();

    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual AppUser User { get; set; } = null!;

    public virtual ICollection<UserRecipeFavorite> UserRecipeFavorites { get; set; } = new List<UserRecipeFavorite>();
}
