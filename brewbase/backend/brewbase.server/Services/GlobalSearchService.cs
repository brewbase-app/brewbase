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

    private readonly BrewDbContext _context;
    private readonly bool _usePostgres;

    public GlobalSearchService(BrewDbContext context)
    {
        _context = context;
        _usePostgres = !(_context.Database.ProviderName ?? string.Empty)
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
        var reader = new GlobalSearchPostgresReader(_context);

        var coffeeTask = reader.SearchCoffeesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var recipeTask = reader.SearchRecipesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var userTask = reader.SearchUsersAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var wikiTask = reader.SearchWikiArticlesAsync(normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var noteTask = reader.SearchQuickNotesAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);
        var cuppingTask = reader.SearchCuppingSessionsAsync(currentUserId, normalizedQuery, ilikePattern, perTypeLimit, cancellationToken);

        await Task.WhenAll(coffeeTask, recipeTask, userTask, wikiTask, noteTask, cuppingTask);

        var merged = new List<GlobalSearchResultDto>();
        merged.AddRange(RefineScores(coffeeTask.Result, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(recipeTask.Result, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(userTask.Result, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(wikiTask.Result, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(noteTask.Result, words, r => r.Title, r => r.Snippet));
        merged.AddRange(RefineScores(cuppingTask.Result, words, r => r.Title, r => r.Snippet));

        return merged;
    }

    private async Task<List<GlobalSearchResultDto>> SearchSqliteAsync(
        int currentUserId,
        string[] words,
        int perTypeLimit,
        CancellationToken cancellationToken)
    {
        var results = new List<GlobalSearchResultDto>();
        results.AddRange(await SearchCoffeesSqliteAsync(words, perTypeLimit, cancellationToken));
        results.AddRange(await SearchRecipesSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken));
        results.AddRange(await SearchUsersSqliteAsync(words, perTypeLimit, cancellationToken));
        results.AddRange(await SearchWikiArticlesSqliteAsync(words, perTypeLimit, cancellationToken));
        results.AddRange(await SearchQuickNotesSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken));
        results.AddRange(await SearchCuppingSessionsSqliteAsync(words, currentUserId, perTypeLimit, cancellationToken));
        return results;
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
        var candidates = await _context.Coffees
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
        var candidates = await RecipeReadService
            .WhereVisibleTo(_context.Recipes.AsNoTracking(), currentUserId)
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
        var candidates = await _context.AppUsers
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
        var candidates = await _context.Articles
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
        var candidates = await _context.QuickNotes
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
                    Path = "/quicknotes",
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
        var candidates = await _context.CuppingSessions
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
