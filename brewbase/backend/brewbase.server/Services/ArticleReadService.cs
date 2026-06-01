using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class ArticleReadService : IArticleReadService
{
    private const string ApprovedStatus = "Approved";
    private const string CoffeeModule = "coffee";

    private readonly BrewDbContext _context;

    public ArticleReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArticleListResponseDto>> GetApprovedAsync(string? module, string? search)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Where(article => article.Status == ApprovedStatus)
            .Where(article =>
                article.Module != CoffeeModule
                || article.CoffeeId == null);

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(article => article.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(article =>
                EF.Functions.ILike(article.Title, $"%{term}%")
                || EF.Functions.ILike(article.Content, $"%{term}%"));
        }

        var articles = await query
            .OrderByDescending(article => article.PublishedAt ?? article.CreatedAt)
            .Select(article => new
            {
                article.Id,
                article.Title,
                article.Module,
                article.Content,
                AuthorLogin = article.User.Login,
                article.PublishedAt,
                article.CoffeeId
            })
            .ToListAsync();

        return articles
            .Select(article =>
            {
                string? beanOriginCountry = null;
                string? variety = null;
                string? processingMethod = null;
                string? region = null;
                string[] flavorProfiles = Array.Empty<string>();

                if (article.Module == "coffee")
                {
                    (beanOriginCountry, variety, processingMethod, flavorProfiles, _) =
                        CoffeeArticleMetadataParser.Parse(article.Content);
                }

                if (article.Module == "country")
                {
                    (region, flavorProfiles) =
                        CountryArticleMetadataParser.Parse(article.Content);
                }

                return new ArticleListResponseDto
                {
                    Id = article.Id,
                    Title = article.Title,
                    Module = article.Module,
                    Content = article.Content,
                    AuthorLogin = article.AuthorLogin,
                    PublishedAt = article.PublishedAt,
                    BeanOriginCountry = beanOriginCountry,
                    Variety = variety,
                    ProcessingMethod = processingMethod,
                    Region = region,
                    FlavorProfiles = flavorProfiles,
                    CoffeeId = article.CoffeeId
                };
            })
            .ToList();
    }

    public async Task<ArticleDetailResponseDto?> GetApprovedByIdAsync(int id)
    {
        return await _context.Articles
            .AsNoTracking()
            .Where(article => article.Id == id && article.Status == ApprovedStatus)
            .Select(article => new ArticleDetailResponseDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Module = article.Module,
                AuthorLogin = article.User.Login,
                PublishedAt = article.PublishedAt,
                CoffeeId = article.CoffeeId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<MyArticleListResponseDto>> GetMineAsync(int userId, string? status)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Where(article => article.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(article => article.Status == status);
        }

        return await query
            .OrderByDescending(article => article.CreatedAt)
            .Select(article => new MyArticleListResponseDto
            {
                Id = article.Id,
                Title = article.Title,
                Module = article.Module,
                Status = article.Status,
                CreatedAt = article.CreatedAt,
                PublishedAt = article.PublishedAt,
                ModerationComment = article.ModerationComment,
                CoffeeId = article.CoffeeId
            })
            .ToListAsync();
    }

    public async Task<MyArticleDetailResponseDto?> GetMineByIdAsync(int id, int userId)
    {
        return await _context.Articles
            .AsNoTracking()
            .Where(article => article.Id == id && article.UserId == userId)
            .Select(article => new MyArticleDetailResponseDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Module = article.Module,
                Status = article.Status,
                CreatedAt = article.CreatedAt,
                PublishedAt = article.PublishedAt,
                ModerationComment = article.ModerationComment,
                CoffeeId = article.CoffeeId
            })
            .FirstOrDefaultAsync();
    }
}
