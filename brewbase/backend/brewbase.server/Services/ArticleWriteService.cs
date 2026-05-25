using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class ArticleWriteService : IArticleWriteService
{
    private static readonly HashSet<string> AllowedModules = new(StringComparer.Ordinal)
    {
        "coffee",
        "country",
        "brewing_method",
        "roastery",
        "general"
    };

    private readonly BrewDbContext _context;

    public ArticleWriteService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<CreateArticleResponseDto?> CreateAsync(int userId, CreateArticleRequestDto request)
    {
        if (!AllowedModules.Contains(request.Module))
        {
            return null;
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var entity = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Module = request.Module,
            Status = "Pending",
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        };

        _context.Articles.Add(entity);
        await _context.SaveChangesAsync();

        return new CreateArticleResponseDto
        {
            Id = entity.Id
        };
    }

    public async Task<ArticleDeleteResult> DeleteMineAsync(int id, int userId)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (article is null)
        {
            return ArticleDeleteResult.NotFound;
        }

        if (article.Status == "Approved")
        {
            return ArticleDeleteResult.NotAllowed;
        }

        var reports = await _context.Reports
            .Where(report => report.ArticleId == id)
            .ToListAsync();

        if (reports.Count > 0)
        {
            _context.Reports.RemoveRange(reports);
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        return ArticleDeleteResult.Deleted;
    }
}
