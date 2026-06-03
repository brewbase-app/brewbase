using System.Linq;
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

    public RegionEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
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

    private static string GetCountryName(BrewDbContext context, int countryId)
    {
        return context.Countries
            .Where(country => country.Id == countryId)
            .Select(country => country.Name)
            .First();
    }
}
