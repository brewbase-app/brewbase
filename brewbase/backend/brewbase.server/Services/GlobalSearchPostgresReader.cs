using brewbase.server.Dtos;
using brewbase.server.Models;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

/// <summary>
/// PostgreSQL-backed global search using brewbase_search_text(), ILIKE and pg_trgm.
/// Indexes in migration 013 use the same brewbase_search_text() expression.
/// </summary>
internal sealed class GlobalSearchPostgresReader
{
    internal const double SimilarityThreshold = 0.2;
    private const string ApprovedStatus = "Approved";

    private readonly BrewDbContext _context;

    public GlobalSearchPostgresReader(BrewDbContext context)
    {
        _context = context;
    }

    public async Task<List<GlobalSearchResultDto>> SearchCoffeesAsync(
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<CoffeeSearchRow>($"""
                SELECT
                    c.id AS "Id",
                    c.name AS "Name",
                    r.name AS "Roastery",
                    reg.name AS "Region",
                    GREATEST(
                        similarity(brewbase_search_text(c.name), brewbase_search_text({normalizedQuery})),
                        similarity(
                            brewbase_search_text(COALESCE(r.name, '') || ' ' || COALESCE(reg.name, '')),
                            brewbase_search_text({normalizedQuery}))
                    ) AS "RankScore"
                FROM coffee c
                INNER JOIN roastery r ON r.id = c.roastery_id
                INNER JOIN region reg ON reg.id = c.region_id
                WHERE brewbase_search_text(c.name) % brewbase_search_text({normalizedQuery})
                   OR similarity(brewbase_search_text(c.name), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                   OR brewbase_search_text(c.name) ILIKE {ilikePattern}
                   OR brewbase_search_text(COALESCE(r.name, '') || ' ' || COALESCE(reg.name, '')) ILIKE {ilikePattern}
                ORDER BY "RankScore" DESC, c.name
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var snippetParts = new[] { row.Roastery, row.Region }
                .Where(part => !string.IsNullOrWhiteSpace(part));

            return new GlobalSearchResultDto
            {
                Id = row.Id,
                Type = "coffee",
                Title = row.Name,
                Snippet = string.Join(" · ", snippetParts),
                Path = $"/wiki/coffees/{row.Id}",
                Score = row.RankScore
            };
        }).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> SearchRecipesAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<RecipeSearchRow>($"""
                SELECT
                    r.id AS "Id",
                    r.title AS "Title",
                    r.steps AS "Steps",
                    r.is_public AS "IsPublic",
                    u.login AS "AuthorLogin",
                    c.name AS "CoffeeName",
                    bm.name AS "MethodName",
                    GREATEST(
                        similarity(brewbase_search_text(r.title), brewbase_search_text({normalizedQuery})),
                        similarity(brewbase_search_text(COALESCE(r.steps, '')), brewbase_search_text({normalizedQuery})) * 0.6
                    ) AS "RankScore"
                FROM recipe r
                INNER JOIN app_user u ON u.id = r.user_id
                LEFT JOIN coffee c ON c.id = r.coffee_id
                LEFT JOIN brewing_method bm ON bm.id = r.brewing_method_id
                WHERE (r.is_public = TRUE OR r.user_id = {currentUserId})
                  AND (
                    brewbase_search_text(r.title) % brewbase_search_text({normalizedQuery})
                    OR similarity(brewbase_search_text(r.title), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                    OR brewbase_search_text(r.title) ILIKE {ilikePattern}
                    OR brewbase_search_text(COALESCE(r.steps, '')) ILIKE {ilikePattern}
                  )
                ORDER BY "RankScore" DESC, r.title
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var visibility = row.IsPublic ? "Publiczna" : "Prywatna";
            return new GlobalSearchResultDto
            {
                Id = row.Id,
                Type = "recipe",
                Title = row.Title ?? "Receptura",
                Snippet = SearchTextNormalizer.BuildSnippet(
                    $"{visibility} · {row.AuthorLogin} · {row.CoffeeName ?? "—"} · {row.MethodName ?? "—"}"),
                Path = $"/recipes/{row.Id}",
                Score = row.RankScore
            };
        }).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> SearchUsersAsync(
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<UserSearchRow>($"""
                SELECT
                    u.id AS "Id",
                    u.login AS "Login",
                    u.label AS "Label",
                    GREATEST(
                        similarity(brewbase_search_text(u.login), brewbase_search_text({normalizedQuery})),
                        similarity(brewbase_search_text(COALESCE(u.label, '')), brewbase_search_text({normalizedQuery})) * 0.7
                    ) AS "RankScore"
                FROM app_user u
                WHERE COALESCE(u.is_blocked, FALSE) = FALSE
                  AND (
                    brewbase_search_text(u.login) % brewbase_search_text({normalizedQuery})
                    OR similarity(brewbase_search_text(u.login), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                    OR brewbase_search_text(u.login) ILIKE {ilikePattern}
                    OR brewbase_search_text(COALESCE(u.label, '')) ILIKE {ilikePattern}
                  )
                ORDER BY "RankScore" DESC, u.login
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new GlobalSearchResultDto
        {
            Id = row.Id,
            Type = "user",
            Title = row.Login,
            Snippet = string.IsNullOrWhiteSpace(row.Label) ? "Profil użytkownika" : row.Label,
            Path = $"/profile/{row.Login}",
            Score = row.RankScore
        }).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> SearchWikiArticlesAsync(
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<WikiSearchRow>($"""
                SELECT
                    a.id AS "Id",
                    a.title AS "Title",
                    a.content AS "Content",
                    a.module AS "Module",
                    a.coffee_id AS "CoffeeId",
                    GREATEST(
                        similarity(brewbase_search_text(a.title), brewbase_search_text({normalizedQuery})),
                        similarity(brewbase_search_text(a.content), brewbase_search_text({normalizedQuery})) * 0.5
                    ) AS "RankScore"
                FROM article a
                WHERE a.status = {ApprovedStatus}
                  AND (
                    brewbase_search_text(a.title) % brewbase_search_text({normalizedQuery})
                    OR similarity(brewbase_search_text(a.title), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                    OR brewbase_search_text(a.title) ILIKE {ilikePattern}
                    OR brewbase_search_text(a.content) ILIKE {ilikePattern}
                  )
                ORDER BY "RankScore" DESC, a.title
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var path = row.Module == "coffee" && row.CoffeeId.HasValue
                ? $"/wiki/coffees/{row.CoffeeId.Value}"
                : $"/wiki/articles/{row.Id}";

            return new GlobalSearchResultDto
            {
                Id = row.Id,
                Type = "wiki",
                Title = row.Title,
                Snippet = SearchTextNormalizer.BuildSnippet(row.Content),
                Path = path,
                Score = row.RankScore
            };
        }).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> SearchQuickNotesAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<QuickNoteSearchRow>($"""
                SELECT
                    q.id AS "Id",
                    q.content AS "Content",
                    similarity(brewbase_search_text(q.content), brewbase_search_text({normalizedQuery})) AS "RankScore"
                FROM quick_note q
                WHERE q.user_id = {currentUserId}
                  AND (
                    brewbase_search_text(q.content) % brewbase_search_text({normalizedQuery})
                    OR similarity(brewbase_search_text(q.content), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                    OR brewbase_search_text(q.content) ILIKE {ilikePattern}
                  )
                ORDER BY "RankScore" DESC, q.updated_at DESC
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var title = SearchTextNormalizer.BuildSnippet(row.Content, 60);
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Quick note";
            }

            return new GlobalSearchResultDto
            {
                Id = row.Id,
                Type = "quick_note",
                Title = title,
                Snippet = SearchTextNormalizer.BuildSnippet(row.Content),
                Path = $"/quicknotes?id={row.Id}",
                Score = row.RankScore
            };
        }).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> SearchCuppingSessionsAsync(
        int currentUserId,
        string normalizedQuery,
        string ilikePattern,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await _context.Database
            .SqlQuery<CuppingSearchRow>($"""
                SELECT
                    cs.id AS "Id",
                    cs.name AS "Name",
                    cs.description AS "Description",
                    GREATEST(
                        similarity(brewbase_search_text(cs.name), brewbase_search_text({normalizedQuery})),
                        similarity(brewbase_search_text(COALESCE(cs.description, '')), brewbase_search_text({normalizedQuery})) * 0.5
                    ) AS "RankScore"
                FROM cupping_session cs
                WHERE cs.user_id = {currentUserId}
                  AND (
                    brewbase_search_text(cs.name) % brewbase_search_text({normalizedQuery})
                    OR similarity(brewbase_search_text(cs.name), brewbase_search_text({normalizedQuery})) > {SimilarityThreshold}
                    OR brewbase_search_text(cs.name) ILIKE {ilikePattern}
                    OR brewbase_search_text(COALESCE(cs.description, '')) ILIKE {ilikePattern}
                  )
                ORDER BY "RankScore" DESC, cs.created_at DESC
                LIMIT {take}
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new GlobalSearchResultDto
        {
            Id = row.Id,
            Type = "cupping",
            Title = row.Name,
            Snippet = SearchTextNormalizer.BuildSnippet(row.Description),
            Path = $"/cupping/{row.Id}",
            Score = row.RankScore
        }).ToList();
    }

    private sealed class CoffeeSearchRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Roastery { get; set; }
        public string? Region { get; set; }
        public double RankScore { get; set; }
    }

    private sealed class RecipeSearchRow
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Steps { get; set; }
        public bool IsPublic { get; set; }
        public string AuthorLogin { get; set; } = null!;
        public string? CoffeeName { get; set; }
        public string? MethodName { get; set; }
        public double RankScore { get; set; }
    }

    private sealed class UserSearchRow
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string? Label { get; set; }
        public double RankScore { get; set; }
    }

    private sealed class WikiSearchRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Module { get; set; } = null!;
        public int? CoffeeId { get; set; }
        public double RankScore { get; set; }
    }

    private sealed class QuickNoteSearchRow
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public double RankScore { get; set; }
    }

    private sealed class CuppingSearchRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double RankScore { get; set; }
    }
}
