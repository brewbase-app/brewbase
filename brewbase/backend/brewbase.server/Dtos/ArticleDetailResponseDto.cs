namespace brewbase.server.Dtos;

public sealed class ArticleDetailResponseDto
{
    public int Id { get; init; }

    public string Title { get; init; } = null!;

    public string Content { get; init; } = null!;

    public string Module { get; init; } = null!;

    public string AuthorLogin { get; init; } = null!;

    public DateTime? PublishedAt { get; init; }

    public int? CoffeeId { get; init; }
}
