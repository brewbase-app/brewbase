using System.Net;
using System.Text.Json;
using brewbase.server.Tests.Infrastructure;
using Xunit;

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
    
    [Fact]
    public async Task ShouldFilterCoffeesByRegionId()
    {
        var response = await _client.GetAsync("/api/Coffee?regionId=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(2, coffees.GetArrayLength());
        Assert.All(coffees.EnumerateArray(), coffee =>
        {
            Assert.Equal("North Region", coffee.GetProperty("region").GetString());
        });
    }

    [Fact]
    public async Task ShouldFilterCoffeesByRoasteryId()
    {
        var response = await _client.GetAsync("/api/Coffee?roasteryId=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(2, coffees.GetArrayLength());
        Assert.All(coffees.EnumerateArray(), coffee =>
        {
            Assert.Equal("Roastery One", coffee.GetProperty("roastery").GetString());
        });
    }

    [Fact(Skip = "Temporary disabled: EF.Functions.ILike is not translated by SQLite in integration tests.")]
    public async Task ShouldSearchCoffeesByName()
    {
        var response = await _client.GetAsync("/api/Coffee?search=beta");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Single(coffees.EnumerateArray());
        Assert.Equal("Beta Coffee", coffees[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldSortCoffeesByNameAscending()
    {
        var response = await _client.GetAsync("/api/Coffee?sortBy=name&sortOrder=asc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        var names = coffees.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Alpha Coffee", "Beta Coffee", "Zulu Coffee" }, names);
    }

    [Fact]
    public async Task ShouldSortCoffeesByNameDescending()
    {
        var response = await _client.GetAsync("/api/Coffee?sortBy=name&sortOrder=desc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        var names = coffees.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Zulu Coffee", "Beta Coffee", "Alpha Coffee" }, names);
    }

    [Fact]
    public async Task ShouldPaginateCoffees()
    {
        var response = await _client.GetAsync("/api/Coffee?page=2&pageSize=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Single(coffees.EnumerateArray());
        Assert.Equal("Beta Coffee", coffees[0].GetProperty("name").GetString());
    }
}