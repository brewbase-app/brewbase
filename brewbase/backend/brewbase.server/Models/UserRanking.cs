using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class UserRanking
{
    public int Id { get; set; }

    public DateTime RefreshedAt { get; set; }

    public int ActivityScore { get; set; }

    public int RecipeCount { get; set; }

    public int LikeCount { get; set; }

    public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
