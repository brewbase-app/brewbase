using Testcontainers.PostgreSql;
using Xunit;

namespace brewbase.server.Tests.Postgres.Infrastructure;

/// <summary>
/// One PostgreSQL 18 container per test collection: schema, search migration, minimal seed.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("brewbase_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await SqlScriptRunner.ExecuteFileAsync(ConnectionString, PostgresDatabasePaths.SchemaSql);
        await SqlScriptRunner.ExecuteFileAsync(
            ConnectionString,
            PostgresDatabasePaths.GlobalSearchMigrationSql);

        await SeedMinimalDataAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private static async Task SeedMinimalDataAsync(string connectionString)
    {
        const string seedSql = """
            INSERT INTO country (name) VALUES ('Ethiopia');

            INSERT INTO region (name, country_id) VALUES ('Yirgacheffe', 1);

            INSERT INTO roastery (name) VALUES ('Postgres Test Roastery');

            INSERT INTO app_user (email, password_hash, login, role, created_at, is_blocked)
            VALUES (
                'postgres.tester@brewbase.local',
                'test-hash',
                'postgres.tester',
                'User',
                CURRENT_TIMESTAMP,
                FALSE);

            INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
            VALUES ('Część Etiopii', 1, 1, 1, TRUE);
            """;

        await SqlScriptRunner.ExecuteScriptAsync(connectionString, seedSql);
    }
}
