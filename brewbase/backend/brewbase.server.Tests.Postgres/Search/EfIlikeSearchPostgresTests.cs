using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Tests.Postgres.Infrastructure;
using Xunit;

namespace brewbase.server.Tests.Postgres.Search;

/// <summary>
/// EF.Functions.ILike search scenarios — not translated correctly on SQLite.
/// </summary>
public sealed class EfIlikeSearchPostgresTests : PostgresIntegrationTestBase
{
    public EfIlikeSearchPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CoffeeList_SearchByName_FiltersWithIlike()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES
                ('Alpha Coffee', 1, 1, 1, TRUE),
                ('Beta Coffee', 1, 1, 1, TRUE);
            """);

        var response = await Client.GetAsync("/api/Coffee?search=beta");
        response.EnsureSuccessStatusCode();

        var coffees = await response.Content.ReadFromJsonAsync<JsonElement>();
        var matches = coffees.EnumerateArray().ToList();

        Assert.Single(matches);
        Assert.Equal("Beta Coffee", matches[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task RecipeList_SearchByTitle_FiltersWithIlike()
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
            VALUES
                (
                    'Alpha Recipe',
                    '{"coffee":"18g","water":"300ml","temperature":"94","brewTime":"3:30"}',
                    'Alpha recipe steps.',
                    TRUE,
                    1,
                    1,
                    1,
                    CURRENT_TIMESTAMP),
                (
                    'Beta Recipe',
                    '{"coffee":"18g","water":"300ml","temperature":"94","brewTime":"3:30"}',
                    'Beta recipe steps.',
                    TRUE,
                    1,
                    1,
                    1,
                    CURRENT_TIMESTAMP);
            """);

        using var request = PostgresTestHttp.CreateGet(
            "/api/Recipe?search=beta",
            PostgresTestSeed.DefaultUserId,
            login: PostgresTestSeed.DefaultUserLogin);

        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var recipes = await response.Content.ReadFromJsonAsync<JsonElement>();
        var matches = recipes.EnumerateArray().ToList();

        Assert.Single(matches);
        Assert.Equal("Beta Recipe", matches[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task QuickNotes_SearchByContent_FiltersWithIlike()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO quick_note (content, created_at, updated_at, user_id)
            VALUES
                ('Other note', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1),
                ('Etiopia jaśminowa', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1),
                ('User2 secret', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3);
            """);

        using var request = PostgresTestHttp.CreateGet(
            "/api/QuickNotes?search=ja%C5%9Bmin",
            PostgresTestSeed.DefaultUserId,
            login: PostgresTestSeed.DefaultUserLogin);

        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var notes = await response.Content.ReadFromJsonAsync<JsonElement>();
        var matches = notes.EnumerateArray().ToList();

        Assert.Single(matches);
        Assert.Contains(
            "jaśmin",
            matches[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
