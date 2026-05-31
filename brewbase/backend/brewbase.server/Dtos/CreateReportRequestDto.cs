namespace brewbase.server.Dtos;

public class CreateReportRequestDto
{
    public string ContentType { get; set; } = default!;

    public int ContentId { get; set; }

    public string? ContentTitle { get; set; }

    public string Category { get; set; } = default!;

    public string? Comment { get; set; }
}
