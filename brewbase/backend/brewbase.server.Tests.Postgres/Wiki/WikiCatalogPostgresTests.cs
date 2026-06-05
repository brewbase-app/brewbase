using System.Net;
using System.Text.Json;
using brewbase.server.Tests.Postgres.Infrastructure;
using Npgsql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Wiki;

public sealed class WikiCatalogPostgresTests : PostgresIntegrationTestBase
{
    public WikiCatalogPostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task ApproveUnlinkedCoffeeArticle_CreatesCatalogCoffeeWithParsedMetadata()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO processing_method (name) VALUES ('Washed');
            INSERT INTO variety (name) VALUES ('Heirloom');

            INSERT INTO article (
                title,
                content,
                module,
                status,
                created_at,
                updated_at,
                user_id)
            VALUES (
                'Unlinked coffee wiki',
                'Kraj pochodzenia ziaren: Ethiopia' || E'\n' ||
                'Odmiana: Heirloom' || E'\n' ||
                'Obróbka ziaren: Washed' || E'\n' ||
                'Palarnia: Hard Beans' || E'\n\n' ||
                'Opis testowej kawy.',
                'coffee',
                'Pending',
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        var articleId = await GetLatestArticleIdAsync();

        using var request = PostgresTestHttp.CreatePatch(
            $"/api/admin/articles/{articleId}/approve",
            PostgresTestSeed.AdminUserId,
            role: "Admin",
            login: PostgresTestSeed.AdminUserLogin);

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var articleCommand = new NpgsqlCommand(
            """
            SELECT coffee_id, status
            FROM article
            WHERE id = @articleId
            """,
            connection);
        articleCommand.Parameters.AddWithValue("articleId", articleId);

        await using (var articleReader = await articleCommand.ExecuteReaderAsync())
        {
            Assert.True(await articleReader.ReadAsync());
            Assert.Equal("Approved", articleReader.GetString(1));
            Assert.False(articleReader.IsDBNull(0));
        }

        await using var coffeeCommand = new NpgsqlCommand(
            """
            SELECT
                c.name,
                r.name AS roastery_name,
                pm.name AS processing_method,
                v.name AS variety,
                reg.name AS region_name
            FROM article a
            JOIN coffee c ON c.id = a.coffee_id
            JOIN roastery r ON r.id = c.roastery_id
            JOIN region reg ON reg.id = c.region_id
            LEFT JOIN processing_method pm ON pm.id = c.processing_method_id
            LEFT JOIN variety v ON v.id = c.variety_id
            WHERE a.id = @articleId
            """,
            connection);
        coffeeCommand.Parameters.AddWithValue("articleId", articleId);

        await using var coffeeReader = await coffeeCommand.ExecuteReaderAsync();
        Assert.True(await coffeeReader.ReadAsync());
        Assert.Equal("Unlinked coffee wiki", coffeeReader.GetString(0));
        Assert.Equal("Hard Beans", coffeeReader.GetString(1));
        Assert.Equal("Washed", coffeeReader.GetString(2));
        Assert.Equal("Heirloom", coffeeReader.GetString(3));
        Assert.Equal("Yirgacheffe", coffeeReader.GetString(4));
    }

    [Fact]
    public async Task ApproveCoffeeArticle_ReusesExistingRoasteryByNormalizedName()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO roastery (name) VALUES ('Hard Beans');

            INSERT INTO article (
                title,
                content,
                module,
                status,
                created_at,
                updated_at,
                user_id)
            VALUES (
                'Another Hard Beans coffee',
                'Kraj pochodzenia ziaren: Ethiopia' || E'\n' ||
                'Palarnia: hard beans' || E'\n\n' ||
                'Opis drugiej kawy.',
                'coffee',
                'Pending',
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                1);
            """);

        var articleId = await GetLatestArticleIdAsync();

        using var request = PostgresTestHttp.CreatePatch(
            $"/api/admin/articles/{articleId}/approve",
            PostgresTestSeed.AdminUserId,
            role: "Admin",
            login: PostgresTestSeed.AdminUserLogin);

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*)::int, MIN(r.id)::int
            FROM roastery r
            WHERE LOWER(TRIM(r.name)) = 'hard beans'
            """,
            connection);

        int roasteryCount;
        int roasteryId;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            roasteryCount = reader.GetInt32(0);
            roasteryId = reader.GetInt32(1);
        }

        Assert.Equal(1, roasteryCount);
        Assert.Equal(2, roasteryId);

        await using var coffeeCommand = new NpgsqlCommand(
            """
            SELECT c.roastery_id
            FROM article a
            JOIN coffee c ON c.id = a.coffee_id
            WHERE a.id = @articleId
            """,
            connection);
        coffeeCommand.Parameters.AddWithValue("articleId", articleId);

        var coffeeRoasteryId = Convert.ToInt32(await coffeeCommand.ExecuteScalarAsync());
        Assert.Equal(roasteryId, coffeeRoasteryId);
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
