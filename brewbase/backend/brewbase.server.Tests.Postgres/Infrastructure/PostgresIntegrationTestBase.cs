using Xunit;

namespace brewbase.server.Tests.Postgres.Infrastructure;

[Collection(PostgresCollection.Name)]
public abstract class PostgresIntegrationTestBase : IAsyncLifetime
{
    protected PostgresFixture Fixture { get; }
    protected PostgresWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected string ConnectionString => Fixture.ConnectionString;

    protected PostgresIntegrationTestBase(PostgresFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await PostgresTestDatabaseReset.ResetToMinimalSeedAsync(ConnectionString);
        Factory = new PostgresWebApplicationFactory(ConnectionString);
        Client = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}
