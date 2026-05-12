using System;

namespace brewbase.server.Models;

public partial class CuppingSessionCoffee
{
    public int Id { get; set; }

    public string? Notes { get; set; }
    
    public int? AromaScore { get; set; }

    public int? SweetnessScore { get; set; }

    public int? AcidityScore { get; set; }

    public int? BodyScore { get; set; }

    public string? FlavorProfileNotes { get; set; }

    public bool? CleanCup { get; set; }

    public int? OverallScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CoffeeId { get; set; }
    
    public string? CustomCoffeeName { get; set; }

    public int CuppingSessionId { get; set; }

    public virtual Coffee? Coffee { get; set; }

    public virtual CuppingSession CuppingSession { get; set; } = null!;
}