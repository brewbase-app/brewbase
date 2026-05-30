using brewbase.server.Services;

namespace brewbase.server.Dtos;

public sealed class ArticleCreateResultDto
{
    public ArticleCreateStatus Status { get; init; }

    public CreateArticleResponseDto? Response { get; init; }
}
