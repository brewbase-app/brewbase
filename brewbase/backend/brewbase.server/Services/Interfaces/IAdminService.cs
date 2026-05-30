
using brewbase.server.Dtos;
using brewbase.server.Models;
namespace brewbase.server.Services.Interfaces;


public interface IAdminService
{
    Task<List<AdminUserListResponseDto>> GetUsersAsync();
    Task<bool> UpdateUserRoleAsync(int userId, string role);
    
    Task<bool> BlockUserAsync(int userId);

    Task<bool> UnblockUserAsync(int userId);
    
    Task<ArticleApproveResultDto> ApproveArticleAsync(int articleId);

    Task<bool> RejectArticleAsync(int articleId, ModerateArticleRequestDto dto);
    
    Task<List<PendingArticleResponseDto>> GetPendingArticlesAsync();
    
    Task<List<ReportedArticleResponseDto>> GetReportsAsync();
    
}