using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
}
