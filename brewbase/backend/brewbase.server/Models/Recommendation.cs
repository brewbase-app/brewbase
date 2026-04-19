using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Recommendation
{
    public int Id { get; set; }

    public bool Feedback { get; set; }

    public int Score { get; set; }

    public string Algorithm { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public DateTime? FeedbackAt { get; set; }

    public string Source { get; set; } = null!;

    public int? CoffeeId { get; set; }

    public int? RecipeId { get; set; }

    public int UserId { get; set; }

    public int? UserPreferenceId { get; set; }

    public int? CoffeeRankingId { get; set; }

    public virtual Coffee? Coffee { get; set; }

    public virtual CoffeeRanking? CoffeeRanking { get; set; }

    public virtual Recipe? Recipe { get; set; }

    public virtual AppUser User { get; set; } = null!;

    public virtual UserPreference? UserPreference { get; set; }
}
