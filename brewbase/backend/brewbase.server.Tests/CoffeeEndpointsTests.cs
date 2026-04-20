using System.Net;
using Xunit;
using System.Text.Json;
using brewbase.server.Tests.Infrastructure;

namespace brewbase.server.Tests;

public class CoffeeEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly HttpClient _client;

    public CoffeeEndpointsTests(CoffeeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnListOfCoffees()
    {
        var response = await _client.GetAsync("/api/Coffee");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(JsonValueKind.Array, coffees.ValueKind);
        Assert.True(coffees.GetArrayLength() > 0);

        var first = coffees[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("isVerified", out _));
        Assert.True(first.TryGetProperty("region", out _));
        Assert.True(first.TryGetProperty("roastery", out _));
        Assert.True(first.TryGetProperty("processingMethod", out _));
        Assert.True(first.TryGetProperty("variety", out _));
        Assert.True(first.TryGetProperty("createdByUserId", out _));
    }

    [Fact]
    public async Task ShouldReturnCoffeeDetailsForValidId()
    {
        var validId = 1;
        var response = await _client.GetAsync($"/api/Coffee/{validId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffee = document.RootElement;

        Assert.Equal(validId, coffee.GetProperty("id").GetInt32());
        Assert.True(coffee.TryGetProperty("name", out _));
        Assert.True(coffee.TryGetProperty("isVerified", out _));
        Assert.True(coffee.TryGetProperty("region", out _));
        Assert.True(coffee.TryGetProperty("roastery", out _));
        Assert.True(coffee.TryGetProperty("processingMethod", out _));
        Assert.True(coffee.TryGetProperty("variety", out _));
        Assert.True(coffee.TryGetProperty("createdByUserId", out _));
    }

    [Fact]
    public async Task ShouldReturnNotFoundForNonExistingId()
    {
        var response = await _client.GetAsync("/api/Coffee/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
