namespace brewbase.server.Dtos;

public sealed class MyArticleDetailResponseDto
{
    public int Id { get; init; }

    public string Title { get; init; } = null!;

    public string Content { get; init; } = null!;

    public string Module { get; init; } = null!;

    public string Status { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public DateTime? PublishedAt { get; init; }

    public string? ModerationComment { get; init; }
}
