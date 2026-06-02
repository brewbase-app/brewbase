using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Body
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Coffee> Coffees { get; set; } = new List<Coffee>();
}