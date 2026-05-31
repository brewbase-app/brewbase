using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class ReportService : IReportService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ReportService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ReportCreateResult> CreateReportAsync(
        CreateReportRequestDto dto)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            return ReportCreateResult.Invalid;

        var contentType = dto.ContentType?.Trim().ToLowerInvariant() ?? "";

        if (!ReportReasonHelper.AllowedContentTypes.Contains(contentType))
            return ReportCreateResult.Invalid;

        if (dto.ContentId <= 0)
            return ReportCreateResult.Invalid;

        var category = dto.Category?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(category) ||
            !ReportReasonHelper.AllowedCategories.Contains(category))
        {
            return ReportCreateResult.Invalid;
        }

        var comment = string.IsNullOrWhiteSpace(dto.Comment)
            ? null
            : dto.Comment.Trim();

        if (comment?.Length > ReportReasonHelper.MaxCommentLength)
            return ReportCreateResult.Invalid;

        var contentTitle = await ResolveContentTitleAsync(
            contentType,
            dto.ContentId,
            dto.ContentTitle);

        if (contentTitle == null)
            return ReportCreateResult.NotFound;

        var articleId = await ResolveArticleIdAsync(contentType, dto.ContentId);

        if (articleId == null)
            return ReportCreateResult.NotFound;

        if (await HasDuplicateReportAsync(userId.Value, contentType, dto.ContentId))
            return ReportCreateResult.Duplicate;

        var payload = new ReportPayload
        {
            ContentType = contentType,
            ContentId = dto.ContentId,
            ContentTitle = contentTitle,
            Category = category,
            Comment = comment,
            Status = ReportStatuses.Open
        };

        var report = new Report
        {
            ArticleId = articleId.Value,
            ReportedByUserId = userId.Value,
            Reason = ReportReasonHelper.Encode(payload),
            CreatedAt = DateTime.Now
        };

        _context.Reports.Add(report);

        await _context.SaveChangesAsync();

        return ReportCreateResult.Created;
    }

    private async Task<string?> ResolveContentTitleAsync(
        string contentType,
        int contentId,
        string? contentTitle)
    {
        if (!string.IsNullOrWhiteSpace(contentTitle))
            return contentTitle.Trim();

        return contentType switch
        {
            "article" => await _context.Articles
                .Where(a => a.Id == contentId)
                .Select(a => a.Title)
                .FirstOrDefaultAsync(),
            "recipe" => await _context.Recipes
                .Where(r => r.Id == contentId)
                .Select(r => r.Title)
                .FirstOrDefaultAsync(),
            "coffee" => await _context.Coffees
                .Where(c => c.Id == contentId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(),
            _ => null
        };
    }

    private async Task<int?> ResolveArticleIdAsync(
        string contentType,
        int contentId)
    {
        switch (contentType)
        {
            case "article":
                return await _context.Articles
                    .AnyAsync(a => a.Id == contentId)
                    ? contentId
                    : null;

            case "coffee":
                if (!await _context.Coffees.AnyAsync(c => c.Id == contentId))
                    return null;

                var linkedArticleId = await _context.Articles
                    .Where(a => a.CoffeeId == contentId)
                    .OrderByDescending(a => a.UpdatedAt)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync();

                return linkedArticleId ?? await GetFallbackArticleIdAsync();

            case "recipe":
                if (!await _context.Recipes.AnyAsync(r => r.Id == contentId))
                    return null;

                return await GetFallbackArticleIdAsync();

            default:
                return null;
        }
    }

    private async Task<int?> GetFallbackArticleIdAsync()
    {
        return await _context.Articles
            .OrderBy(a => a.Id)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> HasDuplicateReportAsync(
        int userId,
        string contentType,
        int contentId)
    {
        var existingReports = await _context.Reports
            .Where(r => r.ReportedByUserId == userId)
            .Select(r => new { r.Reason, r.ArticleId })
            .ToListAsync();

        return existingReports.Any(existing =>
        {
            var payload = ReportReasonHelper.Parse(
                existing.Reason,
                existing.ArticleId);

            if (!ReportReasonHelper.IsOpen(payload))
            {
                return false;
            }

            return string.Equals(
                       payload.ContentType,
                       contentType,
                       StringComparison.OrdinalIgnoreCase) &&
                   payload.ContentId == contentId;
        });
    }
}
