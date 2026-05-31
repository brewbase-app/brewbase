using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public sealed class GlobalSearchService : IGlobalSearchService
{
    private const string ApprovedStatus = "Approved";
    private const string CoffeeModule = "coffee";

    private const int MinQueryLength = 2;
    private const int MaxQueryLength = 120;
    private const int DefaultLimit = 30;
    private const int MaxLimit = 50;
    private const int PerTypeSqlLimit = 25;
    private const int SqlitePrefetchLimit = 40;

    private const double SqlRankWeight = 35;
    private const double ExactTitleBonus = 15;
    private const double StartsWithBonus = 8;

    private readonly IDbContextFactory<BrewDbContext> _contextFactory;
    private readonly bool _usePostgres;

    public GlobalSearchService(IDbContextFactory<BrewDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        using var probeContext = contextFactory.CreateDbContext();
        _usePostgres = !(probeContext.Database.ProviderName ?? string.Empty)
            .Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<GlobalSearchResponseDto> SearchAsync(
        int currentUserId,
        string? query,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var trimmedQuery = query?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedQuery)
            || trimmedQuery.Length < MinQueryLength
            || trimmedQuery.Length > MaxQueryLength)
        {
            return new GlobalSearchResponseDto
            {
                Query = trimmedQuery,
                Results = new List<GlobalSearchResultDto>()
            };
        }

        var words = SearchTextNormalizer.SplitQueryWords(trimmedQuery);
        if (words.Length == 0)
        {
            return new GlobalSearchResponseDto
            {
                Query = trimmedQuery,
                Results = new List<GlobalSearchResultDto>()
            };
        }

        var safeLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
        var perTypeLimit = Math.Clamp(
            Math.Max(safeLimit / 6, 5),
            5,
            PerTypeSqlLimit);

        var normalizedQuery = string.Join(' ', words);
        var ilikePattern = $"%{normalizedQuery}%";

        List<GlobalSearchResultDto> results;

        if (_usePostgres)
        {
            results = await SearchPostgresAsync(
                currentUserId,
                words,
                normalizedQuery,
                ilikePattern,
                perTypeLimit,
                cancellationToken);
        }
        else
        {
            results = await SearchSqliteAsync(
                currentUserId,
                words,
                perTypeLimit,
                cancellationToken);
        }

        return new GlobalSearchResponseDto
        {
            Query = trimmedQuery,
            Results = results
                .Where(result => result.Score > 0)
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.Title)
                .Take(safeLimit)
                .ToList()
        };
    }

    private async Task<List<GlobalSearchResultDto>> SearchPostgresAsync(
        int currentUserId,
        string[] words,
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken)
    {
        var coffeeTask = SearchPostgresCoffeesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var recipeTask = SearchPostgresRecipesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var userTask = SearchPostgresUsersAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var wikiTask = SearchPostgresWikiArticlesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var noteTask = SearchPostgresQuickNotesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var cuppingTask = SearchPostgresCuppingSessionsAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);

        await Task.WhenAll(coffeeTask, recipeTask, userTask, wikiTask, noteTask, cuppingTask);

        var merged = new List<GlobalSearchResultDto>();
        merged.AddRange(RefineScores(await coffeeTask, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(await recipeTask, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(await userTask, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(await wikiTask, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(await noteTask, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(await cuppingTask, words, r => r.Title, r => r.Snippet));

        return merged;
    }

    private async Task<List<GlobalSearchResultDto>> SearchPostgresCoffeesAsync(
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchCoffeesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> SearchPostgresRecipesAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchRecipesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> SearchPostgresUsersAsync(
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchUsersAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> SearchPostgresWikiArticlesAsync(
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchWikiArticlesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> SearchPostgresQuickNotesAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchQuickNotesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> SearchPostgresCuppingSessionsAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int perTypeLimit,
        CancellationToken cancellationToken) =>
        await ExecutePostgresReaderAsync(
            reader => reader.SearchCuppingSessionsAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken),
            cancellationToken);

    private async Task<List<GlobalSearchResultDto>> ExecutePostgresReaderAsync(
        Func<GlobalSearchPostgresReader, Task<List<GlobalSearchResultDto>>> query,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var reader = new GlobalSearchPostgresReader(context);
        return await query(reader);
    }

    private async Task<List<GlobalSearchResultDto>> SearchSqliteAsync(
        int currentUserId,
        string[] words,
        int perTypeLimit,
        CancellationToken cancellationToken)
    {
        var coffeeTask = SearchCoffeesSqliteAsync(words, perTypeLimit, cancellationToken);
        var recipeTask = SearchRecipesSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken);
        var userTask = SearchUsersSqliteAsync(words, perTypeLimit, cancellationToken);
        var wikiTask = SearchWikiArticlesSqliteAsync(words, perTypeLimit, cancellationToken);
        var noteTask = SearchQuickNotesSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken);
        var cuppingTask = SearchCuppingSessionsSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken);

        await Task.WhenAll(coffeeTask, recipeTask, userTask, wikiTask, noteTask, cuppingTask);

        var merged = new List<GlobalSearchResultDto>();
        merged.AddRange(await coffeeTask);
        merged.AddRange(await recipeTask);
        merged.AddRange(await userTask);
        merged.AddRange(await wikiTask);
        merged.AddRange(await noteTask);
        merged.AddRange(await cuppingTask);
        return merged;
    }

    private static List<GlobalSearchResultDto> RefineScores(
        List<GlobalSearchResultDto> candidates,
        string[] words,
        Func<GlobalSearchResultDto, string?> titleSelector,
        Func<GlobalSearchResultDto, string?> bodySelector)
    {
        var refined = new List<GlobalSearchResultDto>(candidates.Count);

        foreach (var candidate in candidates)
        {
            var textScore = SearchTextNormalizer.ScoreMatch(
                words,
                titleSelector(candidate),
                bodySelector(candidate));

            if (textScore <= 0)
            {
                continue;
            }

            var sqlRank = candidate.Score;
            var title = titleSelector(candidate) ?? string.Empty;
            var normalizedTitle = SearchTextNormalizer.Normalize(title);
            var normalizedQuery = string.Join(' ', words);

            if (normalizedTitle == normalizedQuery)
            {
                textScore += ExactTitleBonus;
            }
            else if (normalizedTitle.StartsWith(words[0], StringComparison.Ordinal))
            {
                textScore += StartsWithBonus;
            }

            candidate.Score = sqlRank * SqlRankWeight + textScore;
            refined.Add(candidate);
        }

        return refined;
    }

    private async Task<List<GlobalSearchResultDto>> SearchCoffeesSqliteAsync(
        string[] words,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.Coffees
            .AsNoTracking()
            .Select(coffee => new
            {
                coffee.Id,
                coffee.Name,
                Roastery = coffee.Roastery.Name,
                Region = coffee.Region.Name
            })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(
                    words,
                    candidate.Name,
                    candidate.Roastery,
                    candidate.Region);

                if (score <= 0)
                {
                    return null;
                }

                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "coffee",
                    Title = candidate.Name,
                    Snippet = string.Join(" · ", new[] { candidate.Roastery, candidate.Region }.Where(p => !string.IsNullOrWhiteSpace(p))),
                    Path = $"/wiki/coffees/{candidate.Id}",
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchRecipesSqliteAsync(
        string[] words,
        int currentUserId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await RecipeReadService
            .WhereVisibleTo(context.Recipes.AsNoTracking(), currentUserId)
            .Select(recipe => new
            {
                recipe.Id,
                recipe.Title,
                recipe.Steps,
                recipe.IsPublic,
                AuthorLogin = recipe.User.Login,
                Coffee = recipe.Coffee != null ? recipe.Coffee.Name : null,
                Method = recipe.BrewingMethod != null ? recipe.BrewingMethod.Name : null
            })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(
                    words,
                    candidate.Title,
                    candidate.Steps,
                    candidate.Coffee,
                    candidate.Method);

                if (score <= 0)
                {
                    return null;
                }

                var visibility = candidate.IsPublic ? "Publiczna" : "Prywatna";
                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "recipe",
                    Title = candidate.Title ?? "Receptura",
                    Snippet = SearchTextNormalizer.BuildSnippet(
                        $"{visibility} · {candidate.AuthorLogin} · {candidate.Coffee ?? "—"} · {candidate.Method ?? "—"}"),
                    Path = $"/recipes/{candidate.Id}",
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchUsersSqliteAsync(
        string[] words,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.AppUsers
            .AsNoTracking()
            .Where(user => !user.IsBlocked)
            .Select(user => new { user.Id, user.Login, user.Label })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(words, candidate.Login, candidate.Label);
                if (score <= 0)
                {
                    return null;
                }

                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "user",
                    Title = candidate.Login,
                    Snippet = string.IsNullOrWhiteSpace(candidate.Label) ? "Profil użytkownika" : candidate.Label,
                    Path = $"/profile/{candidate.Login}",
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchWikiArticlesSqliteAsync(
        string[] words,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.Articles
            .AsNoTracking()
            .Where(article => article.Status == ApprovedStatus)
            .Select(article => new
            {
                article.Id,
                article.Title,
                article.Content,
                article.Module,
                article.CoffeeId
            })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(words, candidate.Title, candidate.Content);
                if (score <= 0)
                {
                    return null;
                }

                var path = candidate.Module == CoffeeModule && candidate.CoffeeId.HasValue
                    ? $"/wiki/coffees/{candidate.CoffeeId.Value}"
                    : $"/wiki/articles/{candidate.Id}";

                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "wiki",
                    Title = candidate.Title,
                    Snippet = SearchTextNormalizer.BuildSnippet(candidate.Content),
                    Path = path,
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchQuickNotesSqliteAsync(
        string[] words,
        int currentUserId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.QuickNotes
            .AsNoTracking()
            .Where(note => note.UserId == currentUserId)
            .Select(note => new { note.Id, note.Content })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(words, candidate.Content);
                if (score <= 0)
                {
                    return null;
                }

                var title = SearchTextNormalizer.BuildSnippet(candidate.Content, 60);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = "Quick note";
                }

                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "quick_note",
                    Title = title,
                    Snippet = SearchTextNormalizer.BuildSnippet(candidate.Content),
                    Path = $"/quicknotes?id={candidate.Id}",
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchCuppingSessionsSqliteAsync(
        string[] words,
        int currentUserId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.CuppingSessions
            .AsNoTracking()
            .Where(session => session.UserId == currentUserId)
            .Select(session => new { session.Id, session.Name, session.Description })
            .Take(SqlitePrefetchLimit)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var score = SearchTextNormalizer.ScoreMatch(words, candidate.Name, candidate.Description);
                if (score <= 0)
                {
                    return null;
                }

                return new GlobalSearchResultDto
                {
                    Id = candidate.Id,
                    Type = "cupping",
                    Title = candidate.Name,
                    Snippet = SearchTextNormalizer.BuildSnippet(candidate.Description),
                    Path = $"/cupping/{candidate.Id}",
                    Score = score
                };
            })
            .Where(result => result != null)
            .Cast<GlobalSearchResultDto>()
            .OrderByDescending(result => result.Score)
            .Take(take)
            .ToList();
    }

}
