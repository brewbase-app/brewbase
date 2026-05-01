namespace brewbase.server.Dtos;

public class TastingSessionDetailsResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TastingSessionCoffeeResponseDto> Coffees { get; set; } = [];
}