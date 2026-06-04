using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class UserRanking
{
    public int Id { get; set; }

    public DateTime RefreshedAt { get; set; }

    public int ActivityScore { get; set; }
    
    public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;
    
    public int Position { get; set; }

    public int PublicRecipeCount { get; set; }

    public int CoffeeRatingCount { get; set; }

    public int RecipeRatingCount { get; set; }
    
    public int CuppingSessionCount { get; set; }
    
    public int FollowersCount { get; set; }

    public int ReceivedRecipeFavoriteCount { get; set; }

    public int PublishedArticleCount { get; set; }
}
