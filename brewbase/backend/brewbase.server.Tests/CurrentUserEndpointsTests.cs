using Xunit;
using System.Text.Json;
using brewbase.server.Tests.Infrastructure;

namespace brewbase.server.Tests;

public class CurrentUserEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly HttpClient _client;

    public CurrentUserEndpointsTests(CoffeeApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task ShouldReturnConfiguredDevUserId()
    {
        var response = await _client.GetAsync("/api/CurrentUser");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("userId", out var userIdProperty));
        Assert.Equal(JsonValueKind.Number, userIdProperty.ValueKind);
        Assert.Equal(1, userIdProperty.GetInt32());
    }
}
