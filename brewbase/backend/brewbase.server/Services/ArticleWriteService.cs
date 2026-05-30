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

    public async Task<ArticleCreateResultDto> CreateAsync(int userId, CreateArticleRequestDto request)
    {
        if (!AllowedModules.Contains(request.Module))
        {
            return new ArticleCreateResultDto
            {
                Status = ArticleCreateStatus.InvalidModule
            };
        }

        if (request.CoffeeId.HasValue)
        {
            if (!string.Equals(request.Module, "coffee", StringComparison.Ordinal))
            {
                return new ArticleCreateResultDto
                {
                    Status = ArticleCreateStatus.CoffeeIdNotAllowedForModule
                };
            }

            var coffeeExists = await _context.Coffees
                .AnyAsync(coffee => coffee.Id == request.CoffeeId.Value);

            if (!coffeeExists)
            {
                return new ArticleCreateResultDto
                {
                    Status = ArticleCreateStatus.CoffeeNotFound
                };
            }
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
            UserId = userId,
            CoffeeId = string.Equals(request.Module, "coffee", StringComparison.Ordinal)
                ? request.CoffeeId
                : null
        };

        _context.Articles.Add(entity);
        await _context.SaveChangesAsync();

        return new ArticleCreateResultDto
        {
            Status = ArticleCreateStatus.Success,
            Response = new CreateArticleResponseDto
            {
                Id = entity.Id
            }
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
