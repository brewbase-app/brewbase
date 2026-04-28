using System;

namespace brewbase.server.Models;

public partial class CuppingSessionCoffee
{
    public int Id { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CoffeeId { get; set; }

    public int CuppingSessionId { get; set; }

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual CuppingSession CuppingSession { get; set; } = null!;
}