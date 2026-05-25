using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class RecipeRanking
{
    public int Id { get; set; }

    public DateTime RefreshedAt { get; set; }

    public int RatingCount { get; set; }

    public int LikeCount { get; set; }

    public int SaveCount { get; set; }

    public int RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
    
    public int Position { get; set; }

    public double AverageRating { get; set; }

    public double RankingScore { get; set; }
}
