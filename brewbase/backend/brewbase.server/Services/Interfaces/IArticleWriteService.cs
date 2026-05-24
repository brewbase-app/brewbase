using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IArticleWriteService
{
    Task<CreateArticleResponseDto?> CreateAsync(int userId, CreateArticleRequestDto request);

    Task<ArticleDeleteResult> DeleteMineAsync(int id, int userId);
}
