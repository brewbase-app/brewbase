namespace brewbase.server.Dtos;

public sealed class GlobalSearchResponseDto
{
    public string Query { get; set; } = null!;

    public List<GlobalSearchResultDto> Results { get; set; } = new();
}
