namespace brewbase.server.Dtos;

public class CuppingSessionCoffeeResponseDto
{
    public int SessionCoffeeId { get; set; }
    public int? CoffeeId { get; set; }
    public string CoffeeName { get; set; } = null!;
    public string? Notes { get; set; }
    public int? AromaScore { get; set; }
    public int? SweetnessScore { get; set; }
    public int? AcidityScore { get; set; }
    public int? BodyScore { get; set; }
    public string? FlavorProfileNotes { get; set; }
    public int? OverallScore { get; set; }
}