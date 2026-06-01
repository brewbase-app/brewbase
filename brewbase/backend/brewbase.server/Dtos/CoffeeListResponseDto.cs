namespace brewbase.server.Dtos;

public class CoffeeListResponseDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsVerified { get; set; }

    public string? Region { get; set; }

    public string? Roastery { get; set; }

    public string? ProcessingMethod { get; set; }

    public string? Variety { get; set; }

    public int? CreatedByUserId { get; set; }

    public double? AverageRating { get; set; }

    public int RatingCount { get; set; }

    public string? BeanOriginCountry { get; set; }

    public List<string> FlavorProfiles { get; set; } = new();

    public bool IsFavorite { get; set; }
}
