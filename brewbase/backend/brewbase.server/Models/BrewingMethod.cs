using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class BrewingMethod
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
