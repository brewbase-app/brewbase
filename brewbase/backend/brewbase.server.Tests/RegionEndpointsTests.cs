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

public class RegionEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public RegionEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
        _authenticatedClient = _factory.CreateAuthenticatedClient();
        EnsureRegionNameUniqueIndex();
    }

    public void Dispose()
    {
        _client.Dispose();
        _authenticatedClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldReturnAllRegionsSortedByCountryAndName()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var etiopia = new Country { Name = "Etiopia" };
        var kolumbia = new Country { Name = "Kolumbia" };
        context.Countries.AddRange(etiopia, kolumbia);
        await context.SaveChangesAsync();

        context.Regions.AddRange(
            new Region { Name = "Sidamo", CountryId = etiopia.Id },
            new Region { Name = "Guji", CountryId = etiopia.Id },
            new Region { Name = "Huila", CountryId = kolumbia.Id });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/regions");
        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, regions.ValueKind);
        Assert.True(regions.GetArrayLength() >= 5);

        var entries = regions.EnumerateArray()
            .Select(region => new
            {
                Name = region.GetProperty("name").GetString(),
                CountryId = region.GetProperty("countryId").GetInt32(),
            })
            .ToList();

        var sorted = entries
            .OrderBy(entry => GetCountryName(context, entry.CountryId))
            .ThenBy(entry => entry.Name, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(sorted.Select(entry => entry.Name), entries.Select(entry => entry.Name));

        var guji = regions.EnumerateArray()
            .First(region => region.GetProperty("name").GetString() == "Guji");

        Assert.True(guji.GetProperty("id").GetInt32() > 0);
        Assert.Equal(etiopia.Id, guji.GetProperty("countryId").GetInt32());
    }

    [Fact]
    public async Task ShouldFilterRegionsByCountryId()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var etiopia = await context.Countries
            .SingleAsync(country => country.Name == "Test Country");

        var response = await _client.GetAsync($"/api/regions?countryId={etiopia.Id}");
        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, regions.ValueKind);
        Assert.True(regions.GetArrayLength() >= 1);

        foreach (var region in regions.EnumerateArray())
        {
            Assert.Equal(etiopia.Id, region.GetProperty("countryId").GetInt32());
        }

        var names = regions.EnumerateArray()
            .Select(region => region.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("North Region", names);
        Assert.Contains("South Region", names);
        Assert.DoesNotContain(names, name => name == "Huila");
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenCountryHasNoRegions()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var emptyCountry = new Country { Name = "Z krajem bez regionów" };
        context.Countries.Add(emptyCountry);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/regions?countryId={emptyCountry.Id}");
        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, regions.ValueKind);
        Assert.Equal(0, regions.GetArrayLength());
    }

    [Fact]
    public async Task ShouldRejectSearchWithoutCountryId()
    {
        var response = await _client.GetAsync("/api/regions/search?q=guji");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectSearchForNonExistingCountryId()
    {
        var response = await _client.GetAsync(
            "/api/regions/search?countryId=99999&q=guji");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldSearchRegionsIgnoringDiacriticsAndCase()
    {
        var (countryId, _) = await SeedEtiopiaRegionsAsync();

        var response = await _client.GetAsync(
            $"/api/regions/search?countryId={countryId}&q=yirgacheffe");

        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(regions.GetArrayLength() > 0);

        var first = regions[0];
        Assert.Equal("Yirgacheffe", first.GetProperty("name").GetString());
        Assert.Equal(countryId, first.GetProperty("countryId").GetInt32());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSearchRegionsIgnoringSpaces()
    {
        var (countryId, _) = await SeedEtiopiaRegionsAsync();

        var response = await _client.GetAsync(
            $"/api/regions/search?countryId={countryId}&q=%20%20GUJI%20%20");

        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = regions[0];

        Assert.Equal("Guji", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSuggestFuzzyMatchesForCommonTypos()
    {
        var (countryId, _) = await SeedEtiopiaRegionsAsync();

        var response = await _client.GetAsync(
            $"/api/regions/search?countryId={countryId}&q=sidmo");

        response.EnsureSuccessStatusCode();

        var regions = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = regions[0];

        Assert.Equal("Sidamo", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isFuzzyMatch").GetBoolean());
        Assert.False(first.GetProperty("isExactMatch").GetBoolean());
        Assert.True(first.GetProperty("similarityScore").GetDouble() >= 0.72);
    }

    [Fact]
    public async Task ShouldCreateNewRegion()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var country = new Country { Name = $"Kraj {Guid.NewGuid():N}"[..16] };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var uniqueName = $"Region {Guid.NewGuid():N}"[..20];

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = uniqueName, countryId = country.Id });

        response.EnsureSuccessStatusCode();

        var region = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(region.GetProperty("id").GetInt32() > 0);
        Assert.Equal(uniqueName, region.GetProperty("name").GetString());
        Assert.Equal(country.Id, region.GetProperty("countryId").GetInt32());
    }

    [Fact]
    public async Task ShouldReturnExistingRegionForDuplicateNameInSameCountry()
    {
        var (countryId, _) = await SeedEtiopiaRegionsAsync();
        const string regionName = "Guji";

        var firstResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = regionName, countryId });

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = "  guji  ", countryId });

        duplicateResponse.EnsureSuccessStatusCode();

        var firstRegion = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var duplicateRegion = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            firstRegion.GetProperty("id").GetInt32(),
            duplicateRegion.GetProperty("id").GetInt32());
        Assert.Equal(regionName, duplicateRegion.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingRegionWhenDiacriticsDiffer()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var country = new Country { Name = $"Kolumbia {Guid.NewGuid():N}"[..16] };
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        context.Regions.Add(new Region { Name = "Tarrazú", CountryId = country.Id });
        await context.SaveChangesAsync();

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = "tarrazu", countryId = country.Id });

        response.EnsureSuccessStatusCode();

        var region = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Tarrazú", region.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldAllowSameRegionNameInDifferentCountries()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var countryA = new Country { Name = $"A {Guid.NewGuid():N}"[..10] };
        var countryB = new Country { Name = $"B {Guid.NewGuid():N}"[..10] };
        context.Countries.AddRange(countryA, countryB);
        await context.SaveChangesAsync();

        const string sharedName = "Guji";

        var responseA = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = sharedName, countryId = countryA.Id });

        var responseB = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = sharedName, countryId = countryB.Id });

        responseA.EnsureSuccessStatusCode();
        responseB.EnsureSuccessStatusCode();

        var regionA = await responseA.Content.ReadFromJsonAsync<JsonElement>();
        var regionB = await responseB.Content.ReadFromJsonAsync<JsonElement>();

        Assert.NotEqual(
            regionA.GetProperty("id").GetInt32(),
            regionB.GetProperty("id").GetInt32());
        Assert.Equal(sharedName, regionA.GetProperty("name").GetString());
        Assert.Equal(sharedName, regionB.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldAllowCreatingNewRegionWhenOnlyFuzzyMatchExists()
    {
        var (countryId, _) = await SeedEtiopiaRegionsAsync();

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = "sidmmo", countryId });

        response.EnsureSuccessStatusCode();

        var region = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("sidmmo", region.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldRejectCreateForNonExistingCountryId()
    {
        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/regions",
            new { name = "Guji", countryId = 99999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRejectUnauthenticatedCreateRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/regions",
            new { name = "Guji", countryId = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldPreventDuplicateRegionsDifferingOnlyByCaseAtDatabaseLevel()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var country = new Country { Name = $"DbCase {Guid.NewGuid():N}"[..12] };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var uniqueBase = $"DbRegion {Guid.NewGuid():N}"[..12];
        context.Regions.Add(new Region
        {
            Name = uniqueBase,
            CountryId = country.Id
        });
        await context.SaveChangesAsync();

        context.Regions.Add(new Region
        {
            Name = uniqueBase.ToUpperInvariant(),
            CountryId = country.Id
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());
    }

    private async Task<(int CountryId, int GujiRegionId)> SeedEtiopiaRegionsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var etiopia = await context.Countries
            .FirstOrDefaultAsync(country => country.Name == "Etiopia test regionów");

        if (etiopia == null)
        {
            etiopia = new Country { Name = "Etiopia test regionów" };
            context.Countries.Add(etiopia);
            await context.SaveChangesAsync();

            context.Regions.AddRange(
                new Region { Name = "Guji", CountryId = etiopia.Id },
                new Region { Name = "Sidamo", CountryId = etiopia.Id },
                new Region { Name = "Yirgacheffe", CountryId = etiopia.Id });

            await context.SaveChangesAsync();
        }

        var gujiId = await context.Regions
            .Where(region => region.CountryId == etiopia.Id && region.Name == "Guji")
            .Select(region => region.Id)
            .FirstAsync();

        return (etiopia.Id, gujiId);
    }

    private void EnsureRegionNameUniqueIndex()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Database.ExecuteSqlRaw(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS uq_region_country_name_normalized
                ON region (country_id, LOWER(TRIM(name)));
            """);
    }

    private static string GetCountryName(BrewDbContext context, int countryId)
    {
        return context.Countries
            .Where(country => country.Id == countryId)
            .Select(country => country.Name)
            .First();
    }
}
