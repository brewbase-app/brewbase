using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class AdminService : IAdminService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AdminService(BrewDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<AdminUserListResponseDto>> GetUsersAsync()
    {
        return await _context.AppUsers
            .Select(u => new AdminUserListResponseDto
            {
                Id = u.Id,
                Login = u.Login,
                Role = u.Role,
            })
            .ToListAsync();
    }
    
    public async Task<bool> UpdateUserRoleAsync(int userId, string role)
    {
        var allowedRoles = new[] { "Admin", "User" };

        if (!allowedRoles.Contains(role))
            throw new Exception("Invalid role");

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        user.Role = role;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> BlockUserAsync(int userId)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        user.IsBlocked = true;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnblockUserAsync(int userId)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        user.IsBlocked = false;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> ApproveArticleAsync(int articleId)
    {
        var moderatorId = _currentUserProvider.GetUserId();

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        article.Status = "Approved";
        article.ModeratedAt = DateTime.Now;
        article.ModeratedByUserId = moderatorId;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> RejectArticleAsync(int articleId, ModerateArticleRequestDto dto)
    {
        var moderatorId = _currentUserProvider.GetUserId();

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        article.Status = "Rejected";
        article.ModeratedAt = DateTime.Now;
        article.ModeratedByUserId = moderatorId;
        article.ModerationComment = dto.Comment;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<List<PendingArticleResponseDto>> GetPendingArticlesAsync()
    {
        return await _context.Articles
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new PendingArticleResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorLogin = a.User.Login,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }
    
    public async Task<List<ReportedArticleResponseDto>> GetReportsAsync()
    {
        return await _context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportedArticleResponseDto
            {
                ReportId = r.Id,
                ArticleId = r.ArticleId,
                ArticleTitle = r.Article.Title,
                ReportedBy = r.ReportedByUser.Login,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
}