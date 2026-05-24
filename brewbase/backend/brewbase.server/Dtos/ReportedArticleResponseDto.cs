namespace brewbase.server.Dtos;

public class ReportedArticleResponseDto
{
    public int ReportId { get; set; }

    public int ArticleId { get; set; }

    public string ArticleTitle { get; set; } = default!;

    public string ReportedBy { get; set; } = default!;

    public string Reason { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}