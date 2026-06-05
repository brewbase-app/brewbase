using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Tests.Postgres.Infrastructure;
using Npgsql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Wiki;

public sealed class WikiConstraintPostgresTests : PostgresIntegrationTestBase
{
    public WikiConstraintPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task PartialUniqueIndex_BlocksTwoApprovedWikiForSameCoffee()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO article (
                title,
                content,
                module,
                status,
                coffee_id,
                created_at,
                updated_at,
                published_at,
                user_id)
            VALUES (
                'First approved wiki',
                'Approved wiki content.',
                'coffee',
                'Approved',
                1,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO article (
                title,
                content,
                module,
                status,
                coffee_id,
                created_at,
                updated_at,
                published_at,
                user_id)
            VALUES (
                'Second approved wiki',
                'Competing approved wiki content.',
                'coffee',
                'Approved',
                1,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1)
            """,
            connection);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal("23505", exception.SqlState);
    }

    [Fact]
    public async Task ApproveSecondLinkedCoffeeArticle_ReturnsBadRequestWhenApprovedExists()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO article (
                title,
                content,
                module,
                status,
                coffee_id,
                created_at,
                updated_at,
                published_at,
                user_id)
            VALUES (
                'Existing approved wiki',
                'Already approved content.',
                'coffee',
                'Approved',
                1,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);

            INSERT INTO article (
                title,
                content,
                module,
                status,
                coffee_id,
                created_at,
                updated_at,
                user_id)
            VALUES (
                'Competing pending wiki',
                'Pending competing content.',
                'coffee',
                'Pending',
                1,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        var pendingArticleId = await GetLatestArticleIdAsync();

        using var request = PostgresTestHttp.CreatePatch(
            $"/api/admin/articles/{pendingArticleId}/approve",
            PostgresTestSeed.AdminUserId,
            role: "Admin",
            login: PostgresTestSeed.AdminUserLogin);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(
            "approved wiki article",
            payload.GetProperty("message").GetString(),
            StringComparison.OrdinalIgnoreCase);
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
