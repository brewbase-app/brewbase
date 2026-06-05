using System.Net;
using System.Text.Json;
using brewbase.server.Tests.Postgres.Infrastructure;
using Npgsql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Ranking;

public sealed class RankingRefreshPostgresTests : PostgresIntegrationTestBase
{
    public RankingRefreshPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task RefreshAllRankings_RecomputesCoffeeAverageFromRatings()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Rated Coffee', 1, 1, 1, TRUE);

            INSERT INTO coffee_rating (coffee_id, user_id, value, created_at, updated_at)
            VALUES
                (2, 2, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                (2, 3, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            """);

        await RefreshRankingsAsAdminAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT average_rating, rating_count, position
            FROM coffee_ranking
            WHERE coffee_id = 2
            """,
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(4.5d, reader.GetDouble(0));
        Assert.Equal(2, reader.GetInt32(1));
        Assert.True(reader.GetInt32(2) > 0);
    }

    [Fact]
    public async Task RefreshAllRankings_IncludesFavoriteCount()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Favorite Coffee', 1, 1, 1, TRUE);

            INSERT INTO coffee_rating (coffee_id, user_id, value, created_at, updated_at)
            VALUES (2, 2, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

            INSERT INTO user_coffee_favorite (user_id, coffee_id)
            VALUES (3, 2);
            """);

        await RefreshRankingsAsAdminAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT like_count
            FROM coffee_ranking
            WHERE coffee_id = 2
            """,
            connection);

        var likeCount = await command.ExecuteScalarAsync();
        Assert.Equal(1, Convert.ToInt32(likeCount));
    }

    [Fact]
    public async Task RefreshUserRanking_UpdatesActivityScoreAfterArticleApprove()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            UPDATE app_user
            SET activity_points = 25
            WHERE id = 1;

            INSERT INTO article (
                title,
                content,
                module,
                status,
                created_at,
                updated_at,
                user_id)
            VALUES (
                'Approved country article',
                'Region: Yirgacheffe' || E'\n\n' || 'Country content for ranking test.',
                'country',
                'Pending',
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        var articleId = await GetLatestArticleIdAsync();

        using var approveRequest = PostgresTestHttp.CreatePatch(
            $"/api/admin/articles/{articleId}/approve",
            PostgresTestSeed.AdminUserId,
            role: "Admin",
            login: PostgresTestSeed.AdminUserLogin);

        var approveResponse = await Client.SendAsync(approveRequest);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT activity_score, published_article_count, position
            FROM user_ranking
            WHERE user_id = 1
            """,
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(25, reader.GetInt32(0));
        Assert.Equal(1, reader.GetInt32(1));
        Assert.True(reader.GetInt32(2) > 0);
    }

    private async Task RefreshRankingsAsAdminAsync()
    {
        using var request = PostgresTestHttp.CreatePost(
            "/api/ranking/refresh",
            PostgresTestSeed.AdminUserId,
            role: "Admin",
            login: PostgresTestSeed.AdminUserLogin);

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<int> GetLatestArticleIdAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id FROM article ORDER BY id DESC LIMIT 1",
            connection);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
