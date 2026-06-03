using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class CountryEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public CountryEndpointsTests()
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
    public async Task ShouldReturnAllCountriesSortedByName()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        context.Countries.AddRange(
            new Country { Name = "Kolumbia" },
            new Country { Name = "Etiopia" },
            new Country { Name = "Kenia" });
        await context.SaveChangesAsync();

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
}
