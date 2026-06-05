using System.Net;
using System.Net.Http.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class PreferenceSaveEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public PreferenceSaveEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateAuthenticatedClient();
        SeedFlavorProfiles();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task SavePreferences_WithEmptyFlavorProfileIds_DoesNotCreateFlavorLinks()
    {
        var response = await _client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Średnie",
            flavorProfileIds = Array.Empty<int>(),
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var preference = await context.UserPreferences
            .Include(p => p.UserPreferenceFlavorProfiles)
            .SingleAsync(p => p.UserId == 1);

        Assert.Empty(preference.UserPreferenceFlavorProfiles);
        Assert.True(preference.QuizCompleted);
    }

    [Fact]
    public async Task SavePreferences_WithFlavorProfileIds_CreatesFlavorLinks()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var flavorId = await context.FlavorProfiles
            .OrderBy(profile => profile.Id)
            .Select(profile => profile.Id)
            .FirstAsync();

        var response = await _client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Średnie",
            flavorProfileIds = new[] { flavorId },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var preference = await context.UserPreferences
            .Include(p => p.UserPreferenceFlavorProfiles)
            .SingleAsync(p => p.UserId == 1);

        Assert.Single(preference.UserPreferenceFlavorProfiles);
        Assert.Equal(flavorId, preference.UserPreferenceFlavorProfiles.First().FlavorProfileId);
    }

    [Fact]
    public async Task SavePreferences_Twice_ReplacesFlavorLinksWithoutDuplicates()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (context.FlavorProfiles.Count() < 2)
        {
            context.FlavorProfiles.Add(new FlavorProfile { Name = "Czekolada" });
            context.SaveChanges();
        }

        var flavorIds = await context.FlavorProfiles
            .OrderBy(profile => profile.Id)
            .Select(profile => profile.Id)
            .Take(2)
            .ToListAsync();

        var firstResponse = await _client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Jasne",
            flavorProfileIds = new[] { flavorIds[0] },
        });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Ciemne",
            flavorProfileIds = new[] { flavorIds[1] },
        });
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var preference = await context.UserPreferences
            .Include(p => p.UserPreferenceFlavorProfiles)
            .SingleAsync(p => p.UserId == 1);

        Assert.Single(preference.UserPreferenceFlavorProfiles);
        Assert.Equal(flavorIds[1], preference.UserPreferenceFlavorProfiles.First().FlavorProfileId);
    }

    [Fact]
    public async Task GetPreferences_ReturnsSavedRegionsAndBrewingMethods()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var regionId = await context.Regions
            .OrderBy(region => region.Id)
            .Select(region => region.Id)
            .FirstAsync();

        var brewingMethodId = await context.BrewingMethods
            .OrderBy(method => method.Id)
            .Select(method => method.Id)
            .FirstAsync();

        var saveResponse = await _client.PostAsJsonAsync("/api/preferences", new
        {
            preferredRoastLevel = "Średnie",
            flavorProfileIds = Array.Empty<int>(),
            regionIds = new[] { regionId },
            brewingMethodIds = new[] { brewingMethodId },
        });
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/preferences");
        getResponse.EnsureSuccessStatusCode();

        var payload = await getResponse.Content.ReadFromJsonAsync<PreferencesResponse>();

        Assert.NotNull(payload);
        Assert.Contains("North Region", payload!.Regions);
        Assert.Contains("V60", payload.BrewingMethods);
    }

    private sealed class PreferencesResponse
    {
        public List<string> Regions { get; set; } = [];

        public List<string> BrewingMethods { get; set; } = [];
    }

    private void SeedFlavorProfiles()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (context.FlavorProfiles.Any())
        {
            return;
        }

        context.FlavorProfiles.Add(new FlavorProfile { Name = "Jaśmin" });
        context.SaveChanges();
    }
}
