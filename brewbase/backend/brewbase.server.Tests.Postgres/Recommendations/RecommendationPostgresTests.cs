using System.Net;
using System.Net.Http.Json;
using brewbase.server.Tests.Postgres.Infrastructure;
using Npgsql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Recommendations;

public sealed class RecommendationPostgresTests : PostgresIntegrationTestBase
{
    public RecommendationPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task RefreshRecommendationsForUser_PopulatesRowsAfterPreferencesSaved()
    {
        await SeedRecommendationCatalogAsync();

        PostgresTestHttp.SetAuthenticatedUser(
            Client,
            PostgresTestSeed.DefaultUserId,
            login: PostgresTestSeed.DefaultUserLogin);

        var saveResponse = await Client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Średnie",
            allowExploration = true,
            flavorProfileIds = new[] { 1 },
            regionIds = new[] { 1 },
            brewingMethodIds = new[] { 1 },
            recommendationStyle = "Eksploracyjne"
        });

        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT refresh_recommendations_for_user(1)",
            connection);
        await command.ExecuteNonQueryAsync();

        await using var countCommand = new NpgsqlCommand(
            """
            SELECT COUNT(*)::int
            FROM recommendations
            WHERE user_id = 1
              AND algorithm = 'cron-recommendation-v1'
            """,
            connection);

        var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetRecommendations_ReturnsOrderedMatchesForAuthenticatedUser()
    {
        await SeedRecommendationCatalogAsync();

        PostgresTestHttp.SetAuthenticatedUser(
            Client,
            PostgresTestSeed.DefaultUserId,
            login: PostgresTestSeed.DefaultUserLogin);

        var saveResponse = await Client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Średnie",
            allowExploration = true,
            flavorProfileIds = new[] { 1 },
            regionIds = new[] { 1 },
            brewingMethodIds = new[] { 1 },
            recommendationStyle = "Eksploracyjne"
        });
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var response = await Client.GetAsync("/api/recommendations");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RecommendationPayload>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Coffees);

        Assert.True(payload.Coffees[0].FinalScore >= payload.Coffees[^1].FinalScore);
    }

    private async Task SeedRecommendationCatalogAsync()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO flavor_profile (name) VALUES ('Jasmin');

            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Recommendation Coffee', 1, 1, 1, TRUE);

            INSERT INTO coffee_flavor_profile (coffee_id, flavor_profile_id)
            VALUES (2, 1);

            INSERT INTO coffee_rating (coffee_id, user_id, value, created_at, updated_at)
            VALUES
                (2, 2, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                (2, 3, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

            INSERT INTO user_coffee_favorite (user_id, coffee_id)
            VALUES (2, 2);

            SELECT refresh_all_rankings();
            """);
    }

    private sealed class RecommendationPayload
    {
        public List<CoffeeRecommendationPayload> Coffees { get; set; } = [];

        public List<object> Recipes { get; set; } = [];
    }

    private sealed class CoffeeRecommendationPayload
    {
        public int CoffeeId { get; set; }

        public double FinalScore { get; set; }
    }
}
