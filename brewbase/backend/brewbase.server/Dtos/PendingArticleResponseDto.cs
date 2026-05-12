namespace brewbase.server.Dtos;

public class PendingArticleResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string Content { get; set; } = default!;

    public string AuthorLogin { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}