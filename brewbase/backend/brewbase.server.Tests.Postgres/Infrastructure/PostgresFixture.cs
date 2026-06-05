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

        await PostgresTestSchemaSupplement.ApplyAsync(ConnectionString);
        await SqlScriptRunner.ExecuteFileAsync(
            ConnectionString,
            PostgresDatabasePaths.UserRankingFixMigrationSql);
        await PostgresTestSeed.SeedMinimalDataAsync(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
