using System.Net;
using System.Text.Json;
using brewbase.server.Services;
using brewbase.server.Tests.Postgres.Infrastructure;
using Xunit;

namespace brewbase.server.Tests.Postgres.GlobalSearch;

[Collection(PostgresCollection.Name)]
public sealed class GlobalSearchPostgresTests : IDisposable
{
    private readonly PostgresWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GlobalSearchPostgresTests(PostgresFixture fixture)
    {
        _factory = new PostgresWebApplicationFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Search_MatchesPolishWithoutDiacritics()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/search?query=czesc");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, "1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        var root = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
        var results = root.GetProperty("results").EnumerateArray().ToList();

        Assert.Contains(
            results,
            item =>
                item.GetProperty("type").GetString() == "coffee"
                && item.GetProperty("title").GetString() == "Część Etiopii");
    }
}
