using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class RoasteryEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public RoasteryEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
        _authenticatedClient = _factory.CreateAuthenticatedClient();
        SeedRoasteries();
        EnsureRoasteryNameUniqueIndex();
    }

    public void Dispose()
    {
        _client.Dispose();
        _authenticatedClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldReturnAllRoasteriesSortedByName()
    {
        var response = await _client.GetAsync("/api/roasteries");
        response.EnsureSuccessStatusCode();

        var roasteries = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, roasteries.ValueKind);
        Assert.True(roasteries.GetArrayLength() >= 2);

        var names = roasteries.EnumerateArray()
            .Select(roastery => roastery.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal), names);
        Assert.Contains("Roastery One", names);
        Assert.Contains("Roastery Two", names);

        var first = roasteries[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task ShouldCreateNewRoastery()
    {
        var uniqueName = $"Palarnia {Guid.NewGuid():N}"[..20];

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/roasteries",
            new { name = uniqueName });

        response.EnsureSuccessStatusCode();

        var roastery = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(roastery.GetProperty("id").GetInt32() > 0);
        Assert.Equal(uniqueName, roastery.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingRoasteryForDuplicateName()
    {
        const string roasteryName = "Roastery One";

        var firstResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/roasteries",
            new { name = roasteryName });

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/roasteries",
            new { name = "  roastery one  " });

        duplicateResponse.EnsureSuccessStatusCode();

        var firstRoastery = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var duplicateRoastery = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            firstRoastery.GetProperty("id").GetInt32(),
            duplicateRoastery.GetProperty("id").GetInt32());
        Assert.Equal(roasteryName, duplicateRoastery.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingRoasteryWhenDiacriticsDiffer()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Roasteries.Add(new Roastery { Name = "Kawałek" });
        await context.SaveChangesAsync();

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/roasteries",
            new { name = "kawalek" });

        response.EnsureSuccessStatusCode();

        var roastery = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Kawałek", roastery.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldSearchRoasteriesIgnoringDiacriticsAndCase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Roasteries.Add(new Roastery { Name = "Hard Beans" });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/roasteries/search?q=hard%20beans");
        response.EnsureSuccessStatusCode();

        var roasteries = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, roasteries.ValueKind);
        Assert.True(roasteries.GetArrayLength() > 0);

        var first = roasteries[0];
        Assert.Equal("Hard Beans", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSearchRoasteriesIgnoringSpaces()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Roasteries.Add(new Roastery { Name = "Java Coffee" });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync(
            "/api/roasteries/search?q=%20%20JAVA%20COFFEE%20%20");

        response.EnsureSuccessStatusCode();

        var roasteries = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = roasteries[0];

        Assert.Equal("Java Coffee", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSuggestFuzzyMatchesForCommonTypos()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Roasteries.Add(new Roastery { Name = "CoffeeLab" });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync(
            "/api/roasteries/search?q=coffeelabb");

        response.EnsureSuccessStatusCode();

        var roasteries = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = roasteries[0];

        Assert.Equal("CoffeeLab", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isFuzzyMatch").GetBoolean());
        Assert.False(first.GetProperty("isExactMatch").GetBoolean());
        Assert.True(first.GetProperty("similarityScore").GetDouble() >= 0.72);
    }

    [Fact]
    public async Task ShouldAllowCreatingNewRoasteryWhenOnlyFuzzyMatchExists()
    {
        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/roasteries",
            new { name = "coffeelabb" });

        response.EnsureSuccessStatusCode();

        var roastery = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("coffeelabb", roastery.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldRejectUnauthenticatedCreateRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/roasteries",
            new { name = "Nieautoryzowana palarnia" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldPreventDuplicateRoasteriesDifferingOnlyByCaseAtDatabaseLevel()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var uniqueBase = $"DbCase {Guid.NewGuid():N}"[..12];
        context.Roasteries.Add(new Roastery { Name = uniqueBase });
        await context.SaveChangesAsync();

        context.Roasteries.Add(new Roastery { Name = uniqueBase.ToUpperInvariant() });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());
    }

    private void SeedRoasteries()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (!context.Roasteries.Any())
        {
            context.Roasteries.AddRange(
                new Roastery { Name = "Roastery One" },
                new Roastery { Name = "Roastery Two" });

            context.SaveChanges();
        }
    }

    private void EnsureRoasteryNameUniqueIndex()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Database.ExecuteSqlRaw(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS uq_roastery_name_normalized
                ON roastery (LOWER(TRIM(name)));
            """);
    }
}
