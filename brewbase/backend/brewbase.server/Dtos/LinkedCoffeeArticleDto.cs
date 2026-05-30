namespace brewbase.server.Dtos;

public sealed class LinkedCoffeeArticleDto
{
    public int Id { get; init; }

    public string Content { get; init; } = null!;

    public string AuthorLogin { get; init; } = null!;

    public DateTime? PublishedAt { get; init; }
}
