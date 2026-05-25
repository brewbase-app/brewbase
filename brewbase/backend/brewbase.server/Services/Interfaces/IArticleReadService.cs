using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IArticleReadService
{
    Task<List<ArticleListResponseDto>> GetApprovedAsync(string? module, string? search);

    Task<ArticleDetailResponseDto?> GetApprovedByIdAsync(int id);

    Task<List<MyArticleListResponseDto>> GetMineAsync(int userId, string? status);

    Task<MyArticleDetailResponseDto?> GetMineByIdAsync(int id, int userId);
}
