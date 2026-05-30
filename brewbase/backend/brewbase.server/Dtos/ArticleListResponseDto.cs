namespace brewbase.server.Dtos;

public sealed class ArticleListResponseDto
{
    public int Id { get; init; }

    public string Title { get; init; } = null!;

    public string Module { get; init; } = null!;

    public string AuthorLogin { get; init; } = null!;

    public DateTime? PublishedAt { get; init; }

    public string? BeanOriginCountry { get; init; }

    public string? Variety { get; init; }

    public string? ProcessingMethod { get; init; }

    public string? Region { get; init; }

    public IReadOnlyList<string> FlavorProfiles { get; init; } = Array.Empty<string>();

    public string Content { get; init; } = null!;

    public int? CoffeeId { get; init; }
}
