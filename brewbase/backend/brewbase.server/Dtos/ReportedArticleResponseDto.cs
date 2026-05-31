namespace brewbase.server.Dtos;

public class ReportedArticleResponseDto
{
    public int ReportId { get; set; }

    public int ArticleId { get; set; }

    public string ContentType { get; set; } = default!;

    public int ContentId { get; set; }

    public string ContentTitle { get; set; } = default!;

    public string ArticleTitle { get; set; } = default!;

    public string ReportedBy { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string? Comment { get; set; }

    public string Status { get; set; } = default!;

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedByLogin { get; set; }

    public string? ResolutionAction { get; set; }

    public string Reason { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
