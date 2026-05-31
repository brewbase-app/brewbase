using brewbase.server.Tests.Postgres.Infrastructure;
using Xunit;

namespace brewbase.server.Tests.Postgres;

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
