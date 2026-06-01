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
    
    public async Task<ArticleApproveResultDto> ApproveArticleAsync(int articleId)
    {
        var moderatorId = _currentUserProvider.GetUserId();

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
        {
            return new ArticleApproveResultDto
            {
                Status = ArticleApproveStatus.NotFound
            };
        }

        if (string.Equals(article.Module, "coffee", StringComparison.Ordinal)
            && article.CoffeeId.HasValue
            && !string.Equals(article.Status, "Approved", StringComparison.Ordinal))
        {
            var coffeeAlreadyHasApprovedWiki = await _context.Articles.AnyAsync(existing =>
                existing.Id != article.Id
                && existing.Module == "coffee"
                && existing.Status == "Approved"
                && existing.CoffeeId == article.CoffeeId);

            if (coffeeAlreadyHasApprovedWiki)
            {
                return new ArticleApproveResultDto
                {
                    Status = ArticleApproveStatus.CoffeeAlreadyHasApprovedWiki
                };
            }
        }

        if (string.Equals(article.Module, "coffee", StringComparison.Ordinal)
            && !article.CoffeeId.HasValue)
        {
            article.CoffeeId = await CreateCatalogCoffeeFromArticleAsync(article);
        }

        article.Status = "Approved";
        article.ModeratedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        article.ModeratedByUserId = moderatorId;
        article.PublishedAt = article.ModeratedAt;
        
        _context.Notifications.Add(new Notification
        {
            UserId = article.UserId,
            Content = "Twój artykuł został zatwierdzony.",
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();
        
        return new ArticleApproveResultDto
        {
            Status = ArticleApproveStatus.Approved
        };
    }
    
    public async Task<bool> RejectArticleAsync(int articleId, ModerateArticleRequestDto dto)
    {
        var moderatorId = _currentUserProvider.GetUserId();

        if (moderatorId == null)
            return false;

        var comment = dto.Comment?.Trim();

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        article.Status = "Draft";
        article.ModeratedAt = DateTime.Now;
        article.ModeratedByUserId = moderatorId;
        article.ModerationComment = comment;
        article.PublishedAt = null;
        
        _context.Notifications.Add(new Notification
        {
            UserId = article.UserId,
            Content = $"Twój artykuł został odrzucony. Komentarz moderacji: {comment}",
            CreatedAt = DateTime.Now
        });

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
    
    public async Task<List<ReportedArticleResponseDto>> GetReportsAsync(string scope = "open")
    {
        var reports = await _context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.ArticleId,
                ArticleTitle = r.Article.Title,
                ReportedBy = r.ReportedByUser.Login,
                r.Reason,
                r.CreatedAt
            })
            .ToListAsync();

        var mapped = reports
            .Select(r =>
            {
                var payload = ReportReasonHelper.Parse(
                    r.Reason,
                    r.ArticleId,
                    r.ArticleTitle);

                return new ReportedArticleResponseDto
                {
                    ReportId = r.Id,
                    ArticleId = r.ArticleId,
                    ContentType = payload.ContentType,
                    ContentId = payload.ContentId,
                    ContentTitle = payload.ContentTitle,
                    ArticleTitle = payload.ContentTitle,
                    ReportedBy = r.ReportedBy,
                    Category = payload.Category,
                    Comment = payload.Comment,
                    Status = payload.Status,
                    ResolvedAt = payload.ResolvedAt,
                    ResolvedByLogin = payload.ResolvedByLogin,
                    ResolutionAction = payload.ResolutionAction,
                    Reason = r.Reason,
                    CreatedAt = r.CreatedAt
                };
            })
            .ToList();

        return scope switch
        {
            "history" => mapped
                .Where(report =>
                    report.Status is ReportStatuses.Dismissed or ReportStatuses.Upheld)
                .ToList(),
            "all" => mapped,
            _ => mapped
                .Where(report => report.Status == ReportStatuses.Open)
                .ToList()
        };
    }

    public Task<ReportModerationResult> DismissReportAsync(int reportId)
    {
        return ResolveReportAsync(
            reportId,
            ReportStatuses.Dismissed,
            "dismissed",
            removeContent: false);
    }

    public Task<ReportModerationResult> UpholdReportAsync(int reportId)
    {
        return ResolveReportAsync(
            reportId,
            ReportStatuses.Upheld,
            "content_removed",
            removeContent: true);
    }

    private async Task<ReportModerationResult> ResolveReportAsync(
        int reportId,
        string status,
        string resolutionAction,
        bool removeContent)
    {
        var moderatorId = _currentUserProvider.GetUserId();

        if (moderatorId == null)
            return ReportModerationResult.NotFound;

        var moderatorLogin = await _context.AppUsers
            .Where(user => user.Id == moderatorId.Value)
            .Select(user => user.Login)
            .FirstOrDefaultAsync();

        if (moderatorLogin == null)
            return ReportModerationResult.NotFound;

        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return ReportModerationResult.NotFound;

        var articleTitle = await _context.Articles
            .Where(article => article.Id == report.ArticleId)
            .Select(article => article.Title)
            .FirstOrDefaultAsync();

        var payload = ReportReasonHelper.Parse(
            report.Reason,
            report.ArticleId,
            articleTitle);

        if (!ReportReasonHelper.IsOpen(payload))
            return ReportModerationResult.AlreadyResolved;

        if (removeContent)
        {
            var removed = await RemoveReportedContentAsync(payload);

            if (!removed)
                return ReportModerationResult.ContentNotFound;
        }

        await ResolveMatchingOpenReportsAsync(
            payload.ContentType,
            payload.ContentId,
            status,
            resolutionAction,
            moderatorLogin);

        return ReportModerationResult.Success;
    }

    private async Task ResolveMatchingOpenReportsAsync(
        string contentType,
        int contentId,
        string status,
        string resolutionAction,
        string moderatorLogin)
    {
        var reports = await _context.Reports.ToListAsync();
        var resolvedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        foreach (var report in reports)
        {
            var payload = ReportReasonHelper.Parse(
                report.Reason,
                report.ArticleId);

            if (!ReportReasonHelper.IsOpen(payload))
                continue;

            if (!string.Equals(payload.ContentType, contentType, StringComparison.OrdinalIgnoreCase) ||
                payload.ContentId != contentId)
            {
                continue;
            }

            payload.Status = status;
            payload.ResolvedAt = resolvedAt;
            payload.ResolvedByLogin = moderatorLogin;
            payload.ResolutionAction = resolutionAction;

            report.Reason = ReportReasonHelper.Encode(payload);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<bool> RemoveReportedContentAsync(ReportPayload payload)
    {
        switch (payload.ContentType.ToLowerInvariant())
        {
            case "article":
                return await RemoveReportedArticleAsync(payload.ContentId);

            case "recipe":
                return await RemoveReportedRecipeAsync(payload.ContentId);

            case "coffee":
                return await RemoveReportedCoffeeContentAsync(payload.ContentId);

            default:
                return false;
        }
    }

    private async Task<bool> RemoveReportedArticleAsync(int articleId)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        article.Status = "Removed";
        article.PublishedAt = null;
        article.ModeratedAt = DateTime.Now;
        article.ModeratedByUserId = _currentUserProvider.GetUserId();

        _context.Notifications.Add(new Notification
        {
            UserId = article.UserId,
            Content = "Twój artykuł został usunięty w wyniku moderacji zgłoszenia.",
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<bool> RemoveReportedRecipeAsync(int recipeId)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null)
            return false;

        recipe.IsPublic = false;

        _context.Notifications.Add(new Notification
        {
            UserId = recipe.UserId,
            Content = "Twój przepis został ukryty w wyniku moderacji zgłoszenia.",
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<bool> RemoveReportedCoffeeContentAsync(int coffeeId)
    {
        var coffeeExists = await _context.Coffees
            .AnyAsync(coffee => coffee.Id == coffeeId);

        if (!coffeeExists)
            return false;

        var linkedArticles = await _context.Articles
            .Where(article =>
                article.CoffeeId == coffeeId &&
                article.Status == "Approved")
            .ToListAsync();

        foreach (var article in linkedArticles)
        {
            article.Status = "Removed";
            article.PublishedAt = null;
            article.ModeratedAt = DateTime.Now;
            article.ModeratedByUserId = _currentUserProvider.GetUserId();

            _context.Notifications.Add(new Notification
            {
                UserId = article.UserId,
                Content = "Artykuł wiki powiązany z kawą został usunięty w wyniku moderacji zgłoszenia.",
                CreatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<int> CreateCatalogCoffeeFromArticleAsync(Article article)
    {
        var (beanOriginCountry, varietyName, processingName, _, roasteryName) =
            CoffeeArticleMetadataParser.Parse(article.Content);

        var region = await _context.Regions
            .Include(r => r.Country)
            .FirstOrDefaultAsync(r =>
                beanOriginCountry != null
                && r.Country.Name == beanOriginCountry)
            ?? await _context.Regions
                .OrderBy(r => r.Id)
                .FirstAsync();

        var roasteryId = await ResolveRoasteryIdAsync(roasteryName);

        int? varietyId = null;
        if (!string.IsNullOrWhiteSpace(varietyName))
        {
            varietyId = await _context.Varieties
                .Where(variety => variety.Name == varietyName)
                .Select(variety => (int?)variety.Id)
                .FirstOrDefaultAsync();
        }

        int? processingMethodId = null;
        if (!string.IsNullOrWhiteSpace(processingName))
        {
            processingMethodId = await _context.ProcessingMethods
                .Where(method => method.Name == processingName)
                .Select(method => (int?)method.Id)
                .FirstOrDefaultAsync();
        }

        var coffee = new Coffee
        {
            Name = article.Title,
            RegionId = region.Id,
            RoasteryId = roasteryId,
            VarietyId = varietyId,
            ProcessingMethodId = processingMethodId,
            CreatedByUserId = article.UserId,
            IsVerified = false
        };

        _context.Coffees.Add(coffee);
        await _context.SaveChangesAsync();

        return coffee.Id;
    }

    private async Task<int> ResolveRoasteryIdAsync(string? roasteryName)
    {
        if (string.IsNullOrWhiteSpace(roasteryName))
        {
            return (await _context.Roasteries
                .OrderBy(roastery => roastery.Id)
                .FirstAsync()).Id;
        }

        var trimmedName = roasteryName.Trim();
        var existingId = await _context.Roasteries
            .Where(roastery => roastery.Name == trimmedName)
            .Select(roastery => (int?)roastery.Id)
            .FirstOrDefaultAsync();

        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var roastery = new Roastery
        {
            Name = trimmedName
        };

        _context.Roasteries.Add(roastery);
        await _context.SaveChangesAsync();

        return roastery.Id;
    }
}