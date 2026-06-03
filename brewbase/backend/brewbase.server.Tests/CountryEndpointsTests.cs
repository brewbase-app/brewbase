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

public class CountryEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public CountryEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
        _authenticatedClient = _factory.CreateAuthenticatedClient();
        SeedCountries();
        EnsureCountryNameUniqueIndex();
    }

    public void Dispose()
    {
        _client.Dispose();
        _authenticatedClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldReturnAllCountriesSortedByName()
    {
        var response = await _client.GetAsync("/api/countries");
        response.EnsureSuccessStatusCode();

        var countries = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, countries.ValueKind);
        Assert.True(countries.GetArrayLength() >= 4);

        var names = countries.EnumerateArray()
            .Select(country => country.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal), names);
        Assert.Contains("Etiopia", names);
        Assert.Contains("Kolumbia", names);
        Assert.Contains("Kenia", names);

        var first = countries[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task ShouldCreateNewCountry()
    {
        var uniqueName = $"Kraj {Guid.NewGuid():N}"[..20];

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/countries",
            new { name = uniqueName });

        response.EnsureSuccessStatusCode();

        var country = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(country.GetProperty("id").GetInt32() > 0);
        Assert.Equal(uniqueName, country.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingCountryForDuplicateName()
    {
        const string countryName = "Etiopia";

        var firstResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/countries",
            new { name = countryName });

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/countries",
            new { name = "  etiopia  " });

        duplicateResponse.EnsureSuccessStatusCode();

        var firstCountry = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var duplicateCountry = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            firstCountry.GetProperty("id").GetInt32(),
            duplicateCountry.GetProperty("id").GetInt32());
        Assert.Equal(countryName, duplicateCountry.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingCountryWhenDiacriticsDiffer()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Countries.Add(new Country { Name = "Brazylia" });
        await context.SaveChangesAsync();

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/countries",
            new { name = "brazylia" });

        response.EnsureSuccessStatusCode();

        var country = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Brazylia", country.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldSearchCountriesIgnoringDiacriticsAndCase()
    {
        var response = await _client.GetAsync("/api/countries/search?q=etiopia");
        response.EnsureSuccessStatusCode();

        var countries = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, countries.ValueKind);
        Assert.True(countries.GetArrayLength() > 0);

        var first = countries[0];
        Assert.Equal("Etiopia", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSearchCountriesIgnoringSpaces()
    {
        var response = await _client.GetAsync(
            "/api/countries/search?q=%20%20KENIA%20%20");

        response.EnsureSuccessStatusCode();

        var countries = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = countries[0];

        Assert.Equal("Kenia", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isExactMatch").GetBoolean());
    }

    [Fact]
    public async Task ShouldSuggestFuzzyMatchesForCommonTypos()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Countries.Add(new Country { Name = "Gwatemala" });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync(
            "/api/countries/search?q=gwatemela");

        response.EnsureSuccessStatusCode();

        var countries = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = countries[0];

        Assert.Equal("Gwatemala", first.GetProperty("name").GetString());
        Assert.True(first.GetProperty("isFuzzyMatch").GetBoolean());
        Assert.False(first.GetProperty("isExactMatch").GetBoolean());
        Assert.True(first.GetProperty("similarityScore").GetDouble() >= 0.72);
    }

    [Fact]
    public async Task ShouldAllowCreatingNewCountryWhenOnlyFuzzyMatchExists()
    {
        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/countries",
            new { name = "kolumbja" });

        response.EnsureSuccessStatusCode();

        var country = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("kolumbja", country.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldRejectUnauthenticatedCreateRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/countries",
            new { name = "Nieautoryzowany kraj" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldPreventDuplicateCountriesDifferingOnlyByCaseAtDatabaseLevel()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var uniqueBase = $"DbCase {Guid.NewGuid():N}"[..12];
        context.Countries.Add(new Country { Name = uniqueBase });
        await context.SaveChangesAsync();

        context.Countries.Add(new Country { Name = uniqueBase.ToUpperInvariant() });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());
    }

    private void SeedCountries()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (context.Countries.Any(country =>
                country.Name == "Etiopia"
                || country.Name == "Kenia"
                || country.Name == "Kolumbia"))
        {
            return;
        }

        context.Countries.AddRange(
            new Country { Name = "Etiopia" },
            new Country { Name = "Kenia" },
            new Country { Name = "Kolumbia" });

        context.SaveChanges();
    }

    private void EnsureCountryNameUniqueIndex()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Database.ExecuteSqlRaw(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS uq_country_name_normalized
                ON country (LOWER(TRIM(name)));
            """);
    }
}
