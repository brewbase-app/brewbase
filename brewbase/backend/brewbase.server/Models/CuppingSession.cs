using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class CuppingSession
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<CuppingSessionCoffee> CuppingSessionCoffees { get; set; } = new List<CuppingSessionCoffee>();

    public virtual AppUser User { get; set; } = null!;
}
