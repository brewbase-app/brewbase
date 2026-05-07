namespace brewbase.server.Dtos;

public class CoffeeDetailResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsVerified { get; set; }

    public string? Region { get; set; }

    public string? Roastery { get; set; }

    public string? ProcessingMethod { get; set; }

    public string? Variety { get; set; }

    public int CreatedByUserId { get; set; }
    
    public double? AverageRating { get; set; }

    public int RatingCount { get; set; }
}
