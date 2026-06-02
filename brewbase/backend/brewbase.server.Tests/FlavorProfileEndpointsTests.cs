using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class FlavorProfileEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public FlavorProfileEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
        _authenticatedClient = _factory.CreateAuthenticatedClient();

        SeedFlavorProfiles();
    }

    public void Dispose()
    {
        _client.Dispose();
        _authenticatedClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldReturnAllFlavorProfilesSortedByName()
    {
        var response = await _client.GetAsync("/api/flavor-profiles");
        response.EnsureSuccessStatusCode();

        var profiles = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, profiles.ValueKind);
        Assert.True(profiles.GetArrayLength() >= 3);

        var names = profiles.EnumerateArray()
            .Select(profile => profile.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal), names);
        Assert.Contains("Czekolada", names);
    }

    [Fact]
    public async Task ShouldCreateNewFlavorProfile()
    {
        var uniqueName = $"Profil {Guid.NewGuid():N}"[..20];

        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/flavor-profiles",
            new { name = uniqueName });

        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(profile.GetProperty("id").GetInt32() > 0);
        Assert.Equal(uniqueName, profile.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExistingFlavorProfileForDuplicateName()
    {
        const string profileName = "Jaśmin";

        var firstResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/flavor-profiles",
            new { name = profileName });

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/flavor-profiles",
            new { name = "  jaśmin  " });

        duplicateResponse.EnsureSuccessStatusCode();

        var firstProfile = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var duplicateProfile = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            firstProfile.GetProperty("id").GetInt32(),
            duplicateProfile.GetProperty("id").GetInt32());
        Assert.Equal(profileName, duplicateProfile.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnRandomFlavorProfilesLimitedToRequestedCount()
    {
        var response = await _client.GetAsync("/api/flavor-profiles/random?limit=2");
        response.EnsureSuccessStatusCode();

        var profiles = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, profiles.ValueKind);
        Assert.Equal(2, profiles.GetArrayLength());
    }

    [Fact]
    public async Task ShouldReturnAllFlavorProfilesWhenRandomLimitExceedsAvailableCount()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var totalCount = await context.FlavorProfiles.CountAsync();

        var response = await _client.GetAsync(
            $"/api/flavor-profiles/random?limit={totalCount + 5}");

        response.EnsureSuccessStatusCode();

        var profiles = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(totalCount, profiles.GetArrayLength());
    }

    [Fact]
    public async Task ShouldRejectUnauthenticatedCreateRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/flavor-profiles",
            new { name = "Nieautoryzowany profil" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private void SeedFlavorProfiles()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (context.FlavorProfiles.Any())
        {
            return;
        }

        context.FlavorProfiles.AddRange(
            new FlavorProfile { Name = "Jaśmin" },
            new FlavorProfile { Name = "Cytrusy" },
            new FlavorProfile { Name = "Czekolada" },
            new FlavorProfile { Name = "Karmel" });

        context.SaveChanges();
    }
}
