using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RecipeReadService : IRecipeReadService
{
    private readonly BrewDbContext _context;

    public RecipeReadService(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecipeListResponseDto>> GetAllAsync(
        int? coffeeId,
        int? userId,
        int? brewingMethodId,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize,
        int currentUserId)
    {
        var query = WhereVisibleTo(_context.Recipes.AsNoTracking(), currentUserId);

        if (coffeeId.HasValue)
        {
            query = query.Where(r => r.CoffeeId == coffeeId.Value);
        }

        // After visibility: filtering by owner id cannot expose others' private recipes.
        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (brewingMethodId.HasValue)
        {
            query = query.Where(r => r.BrewingMethodId == brewingMethodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Title != null && EF.Functions.ILike(r.Title, $"%{search}%"));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var isTitleSort = string.Equals(sortBy, "title", StringComparison.OrdinalIgnoreCase);

        query = isTitleSort
            ? (isDesc ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title))
            : query.OrderBy(r => r.Id);

        var safePage = Math.Max(page ?? 1, 1);
        var safePageSize = Math.Clamp(pageSize ?? 10, 1, 100);

        var skip = (safePage - 1) * safePageSize;
        query = query.Skip(skip).Take(safePageSize);

        var recipes = await query
            .Select(r => new RecipeListResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                CoffeeId = r.CoffeeId,
                BrewingMethodId = r.BrewingMethodId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null,
                CreatedAt = r.CreatedAt,
                ModerationComment = r.ModerationComment,
                IsFavorite = _context.UserRecipeFavorites.Any(f =>
                    f.UserId == currentUserId && f.RecipeId == r.Id)
            })
            .ToListAsync();

        await ApplyModerationCommentFallbacksAsync(recipes);

        return recipes;
    }

    public async Task<RecipeDetailResponseDto?> GetByIdAsync(int id, int currentUserId)
    {
        var recipe = await WhereVisibleTo(_context.Recipes.AsNoTracking(), currentUserId)
            .Where(r => r.Id == id)
            .Select(r => new RecipeDetailResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                Parameters = r.Parameters,
                Steps = r.Steps,
                IsPublic = r.IsPublic,
                UserId = r.UserId,
                CoffeeId = r.CoffeeId,
                BrewingMethodId = r.BrewingMethodId,
                BrewingMethod = r.BrewingMethod != null ? r.BrewingMethod.Name : null,
                Coffee = r.Coffee != null ? r.Coffee.Name : null,
                CreatedAt = r.CreatedAt,
                ModerationComment = r.ModerationComment,
                AverageRating = _context.RecipeRatings
                    .Where(rating => rating.RecipeId == r.Id)
                    .Average(rating => (double?)rating.Value),
                RatingCount = _context.RecipeRatings
                    .Count(rating => rating.RecipeId == r.Id),
                IsFavorite = _context.UserRecipeFavorites.Any(f =>
                    f.UserId == currentUserId && f.RecipeId == r.Id)
            })
            .FirstOrDefaultAsync();

        if (recipe != null)
        {
            await ApplyModerationCommentFallbackAsync(recipe);
        }

        return recipe;
    }

    private async Task ApplyModerationCommentFallbacksAsync(IList<RecipeListResponseDto> recipes)
    {
        if (!recipes.Any(recipe =>
                string.IsNullOrWhiteSpace(recipe.ModerationComment) && !recipe.IsPublic))
        {
            return;
        }

        var reports = await _context.Reports
            .AsNoTracking()
            .Select(report => new ReportReference(report.Reason, report.ArticleId))
            .ToListAsync();

        foreach (var recipe in recipes)
        {
            if (string.IsNullOrWhiteSpace(recipe.ModerationComment) && !recipe.IsPublic)
            {
                recipe.ModerationComment = ResolveModerationCommentFromReports(
                    recipe.Id,
                    reports)
                    ?? await ResolveModerationCommentFromNotificationsAsync(recipe.UserId);
            }
        }
    }

    private async Task ApplyModerationCommentFallbackAsync(RecipeDetailResponseDto recipe)
    {
        if (!string.IsNullOrWhiteSpace(recipe.ModerationComment) || recipe.IsPublic)
        {
            return;
        }

        var reports = await _context.Reports
            .AsNoTracking()
            .Select(report => new ReportReference(report.Reason, report.ArticleId))
            .ToListAsync();

        recipe.ModerationComment = ResolveModerationCommentFromReports(recipe.Id, reports)
            ?? await ResolveModerationCommentFromNotificationsAsync(recipe.UserId);
    }

    private static string? ResolveModerationCommentFromReports(
        int recipeId,
        IEnumerable<ReportReference> reports)
    {
        return reports
            .Select(report =>
                ReportReasonHelper.Parse(report.Reason, report.ArticleId))
            .Where(payload =>
                string.Equals(payload.ContentType, "recipe", StringComparison.OrdinalIgnoreCase) &&
                payload.ContentId == recipeId &&
                payload.Status == ReportStatuses.Upheld &&
                !string.IsNullOrWhiteSpace(payload.ModerationComment))
            .OrderByDescending(payload => payload.ResolvedAt ?? DateTime.MinValue)
            .Select(payload => payload.ModerationComment)
            .FirstOrDefault();
    }

    private async Task<string?> ResolveModerationCommentFromNotificationsAsync(int recipeOwnerId)
    {
        const string marker = "Komentarz moderacji:";

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == recipeOwnerId &&
                EF.Functions.ILike(notification.Content, "%przepis%") &&
                EF.Functions.ILike(notification.Content, $"%{marker}%"))
            .OrderByDescending(notification => notification.CreatedAt)
            .Select(notification => notification.Content)
            .Take(5)
            .ToListAsync();

        foreach (var content in notifications)
        {
            var markerIndex = content.IndexOf(marker, StringComparison.Ordinal);

            if (markerIndex < 0)
            {
                continue;
            }

            var comment = content[(markerIndex + marker.Length)..].Trim();

            if (!string.IsNullOrWhiteSpace(comment))
            {
                return comment;
            }
        }

        return null;
    }

    private sealed record ReportReference(string Reason, int ArticleId);

    /// <summary>
    /// Public recipes, or private recipes owned by <paramref name="currentUserId"/>.
    /// Other users' private recipes are excluded (same rule as PUT/DELETE lookups).
    /// </summary>
    internal static IQueryable<Recipe> WhereVisibleTo(IQueryable<Recipe> query, int currentUserId)
    {
        return query.Where(r => r.IsPublic || r.UserId == currentUserId);
    }
}