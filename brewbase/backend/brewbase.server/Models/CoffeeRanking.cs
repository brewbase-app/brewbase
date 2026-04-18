using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class CoffeeRanking
{
    public int Id { get; set; }

    public DateTime RefreshedAt { get; set; }

    public int RatingCount { get; set; }

    public int RecipeUsedCount { get; set; }

    public int LikeCount { get; set; }

    public int CoffeeId { get; set; }

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}
