namespace brewbase.server.Dtos;

public class TastingSessionCoffeeResponseDto
{
    public int CoffeeId { get; set; }
    public string CoffeeName { get; set; } = null!;
    public string? Notes { get; set; }
}