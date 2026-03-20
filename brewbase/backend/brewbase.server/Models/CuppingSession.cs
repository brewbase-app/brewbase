using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class CuppingSession
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<Coffee> Coffees { get; set; } = new List<Coffee>();

    public virtual AppUser User { get; set; } = null!;
}
