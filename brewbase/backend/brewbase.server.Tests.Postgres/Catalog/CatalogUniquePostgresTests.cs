using brewbase.server.Tests.Postgres.Infrastructure;
using Npgsql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Catalog;

public sealed class CatalogUniquePostgresTests : PostgresIntegrationTestBase
{
    public CatalogUniquePostgresTests(PostgresFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CountryCreate_PreventsCaseOnlyDuplicate()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO country (name)
            VALUES ('ETHIOPIA')
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    [Fact]
    public async Task CountryCreate_PreventsTrimmedDuplicate()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO country (name)
            VALUES ('  ethiopia  ')
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    [Fact]
    public async Task RoasteryCreate_PreventsCaseOnlyDuplicateAtDatabaseLevel()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO roastery (name)
            VALUES ('POSTGRES TEST ROASTERY')
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    [Fact]
    public async Task RoasteryCreate_PreventsTrimmedDuplicateAtDatabaseLevel()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO roastery (name)
            VALUES ('  Postgres Test Roastery  ')
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    [Fact]
    public async Task RegionCreate_BlocksDuplicateWithinCountry()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO region (name, country_id)
            VALUES ('  yirgacheffe  ', 1)
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    [Fact]
    public async Task RegionCreate_AllowsSameNameInDifferentCountries()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO country (name) VALUES ('Colombia');

            INSERT INTO region (name, country_id)
            VALUES ('Yirgacheffe', 2);
            """);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT COUNT(*)::int
            FROM region
            WHERE LOWER(TRIM(name)) = 'yirgacheffe'
            """,
            connection);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task FlavorProfileCreate_PreventsCaseOnlyDuplicateAtDatabaseLevel()
    {
        await SqlScriptRunner.ExecuteScriptAsync(
            ConnectionString,
            """
            INSERT INTO flavor_profile (name) VALUES ('Jasmin');
            """);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO flavor_profile (name)
            VALUES ('  JASMIN  ')
            """,
            connection);

        await AssertUniqueViolationAsync(command);
    }

    private static async Task AssertUniqueViolationAsync(NpgsqlCommand command)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => command.ExecuteNonQueryAsync());

        Assert.Equal("23505", exception.SqlState);
    }
}
