namespace brewbase.server.Dtos;

public class CuppingSessionListItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CoffeeCount { get; set; }
    public DateTime? SessionDate { get; set; }
}