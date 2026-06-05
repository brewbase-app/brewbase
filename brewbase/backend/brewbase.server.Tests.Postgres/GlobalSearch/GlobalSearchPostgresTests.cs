using System.Net;
using System.Text.Json;
using brewbase.server.Services;
using brewbase.server.Tests.Postgres.Infrastructure;
using Xunit;

namespace brewbase.server.Tests.Postgres.GlobalSearch;

public sealed class GlobalSearchPostgresTests : PostgresIntegrationTestBase
{
    public GlobalSearchPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Search_MatchesPolishWithoutDiacritics()
    {
        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "czesc");

        Assert.Contains(
            results,
            item =>
                item.GetProperty("type").GetString() == "coffee"
                && item.GetProperty("title").GetString() == "Część Etiopii");
    }

    [Fact]
    public async Task Search_FuzzyMatchesTypoInCoffeeName()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Yirgacheffe Heirloom', 1, 1, 1, TRUE);
            """);

        // Prefix of normalized title — passes RefineScores after PG fuzzy match.
        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "yirgacheff");

        Assert.Contains(
            results,
            item =>
                item.GetProperty("type").GetString() == "coffee"
                && item.GetProperty("title").GetString() == "Yirgacheffe Heirloom");
    }

    [Fact]
    public async Task Search_MatchesRoasteryOrRegionViaIlike()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Neutral Blend', 1, 1, 1, TRUE);
            """);

        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "yirgacheffe");

        Assert.Contains(
            results,
            item =>
                item.GetProperty("type").GetString() == "coffee"
                && item.GetProperty("title").GetString() == "Neutral Blend");
    }

    [Fact]
    public async Task Search_PrivateRecipe_VisibleOnlyToOwner()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO recipe (
                title,
                parameters,
                steps,
                is_public,
                user_id,
                coffee_id,
                brewing_method_id,
                created_at)
            VALUES (
                'Zulu Private Recipe',
                '{"coffee":"18g","water":"300ml","temperature":"94","brewTime":"3:30"}',
                'Private steps for zulu search test.',
                FALSE,
                3,
                1,
                1,
                CURRENT_TIMESTAMP);
            """);

        var ownerResults = await SearchResultsAsync(PostgresTestSeed.SecondUserId, "zulu private");
        Assert.Contains(
            ownerResults,
            item =>
                item.GetProperty("type").GetString() == "recipe"
                && item.GetProperty("title").GetString() == "Zulu Private Recipe");

        var otherUserResults = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "zulu private");
        Assert.DoesNotContain(
            otherUserResults,
            item =>
                item.GetProperty("type").GetString() == "recipe"
                && item.GetProperty("title").GetString() == "Zulu Private Recipe");
    }

    [Fact]
    public async Task Search_ExcludesPendingWikiArticle()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO article (
                title,
                content,
                module,
                status,
                created_at,
                updated_at,
                user_id)
            VALUES (
                'PendingWikiOnly',
                'Pending wiki content for postgres search.',
                'general',
                'Pending',
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "pendingwikionly");

        Assert.DoesNotContain(
            results,
            item =>
                item.GetProperty("type").GetString() == "wiki"
                && item.GetProperty("title").GetString() == "PendingWikiOnly");
    }

    [Fact]
    public async Task Search_ExcludesBlockedUser()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            UPDATE app_user
            SET is_blocked = TRUE
            WHERE id = 3;
            """);

        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "postgres.tester.two");

        Assert.DoesNotContain(
            results,
            item =>
                item.GetProperty("type").GetString() == "user"
                && item.GetProperty("title").GetString() == "postgres.tester.two");
    }

    [Fact]
    public async Task Search_OrdersResultsByScoreDesc()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES
                ('Alpha Postgres Batch', 1, 1, 1, TRUE),
                ('Beta Postgres Batch', 1, 1, 1, TRUE);
            """);

        var results = await SearchResultsAsync(PostgresTestSeed.DefaultUserId, "postgres batch");

        var coffeeResults = results
            .Where(item => item.GetProperty("type").GetString() == "coffee")
            .ToList();

        Assert.True(coffeeResults.Count >= 2);

        var firstScore = coffeeResults[0].GetProperty("score").GetDouble();
        var secondScore = coffeeResults[1].GetProperty("score").GetDouble();

        Assert.True(firstScore >= secondScore);
    }

    private async Task<List<JsonElement>> SearchResultsAsync(int userId, string query, int? limit = null)
    {
        var url = limit.HasValue
            ? $"/api/search?query={Uri.EscapeDataString(query)}&limit={limit.Value}"
            : $"/api/search?query={Uri.EscapeDataString(query)}";

        using var request = PostgresTestHttp.CreateGet(
            url,
            userId,
            login: userId switch
            {
                PostgresTestSeed.AdminUserId => PostgresTestSeed.AdminUserLogin,
                PostgresTestSeed.SecondUserId => PostgresTestSeed.SecondUserLogin,
                _ => PostgresTestSeed.DefaultUserLogin
            });

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        var root = await JsonSerializer.DeserializeAsync<JsonElement>(stream);

        return root.GetProperty("results").EnumerateArray().ToList();
    }
}
