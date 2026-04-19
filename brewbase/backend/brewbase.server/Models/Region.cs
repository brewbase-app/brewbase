using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Region
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CountryId { get; set; }

    public virtual ICollection<Coffee> Coffees { get; set; } = new List<Coffee>();

    public virtual Country Country { get; set; } = null!;
}
