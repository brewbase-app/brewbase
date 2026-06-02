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
            allowExploration = false,
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
            allowExploration = true,
            flavorProfileIds = new[] { flavorId },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var preference = await context.UserPreferences
            .Include(p => p.UserPreferenceFlavorProfiles)
            .SingleAsync(p => p.UserId == 1);

        Assert.Single(preference.UserPreferenceFlavorProfiles);
        Assert.Equal(flavorId, preference.UserPreferenceFlavorProfiles.First().FlavorProfileId);
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
