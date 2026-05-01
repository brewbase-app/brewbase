using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Coffee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int RoasteryId { get; set; }

    public int RegionId { get; set; }

    public int? VarietyId { get; set; }

    public int? ProcessingMethodId { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsVerified { get; set; }
    
    public virtual ICollection<CoffeeRanking> CoffeeRankings { get; set; } = new List<CoffeeRanking>();

    public virtual ICollection<CoffeeRating> CoffeeRatings { get; set; } = new List<CoffeeRating>();
    
    public virtual ICollection<CuppingSessionCoffee> CuppingSessionCoffees { get; set; } = new List<CuppingSessionCoffee>();

    public virtual AppUser CreatedByUser { get; set; } = null!;
    
    public virtual ProcessingMethod? ProcessingMethod { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual Region Region { get; set; } = null!;

    public virtual Roastery Roastery { get; set; } = null!;

    public virtual Variety? Variety { get; set; }
}
