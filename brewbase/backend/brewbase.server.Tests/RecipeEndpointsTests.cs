using System.Net;
using System.Text.Json;
using brewbase.server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace brewbase.server.Tests;

public class RecipeEndpointsTests : IClassFixture<RecipeApiFactory>
{
    private readonly HttpClient _client;

    public RecipeEndpointsTests(RecipeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnListOfRecipes()
    {
        var response = await _client.GetAsync("/api/Recipe");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Equal(JsonValueKind.Array, recipes.ValueKind);
        Assert.True(recipes.GetArrayLength() > 0);

        var first = recipes[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.True(first.TryGetProperty("title", out _));
        Assert.True(first.TryGetProperty("parameters", out _));
        Assert.True(first.TryGetProperty("steps", out _));
        Assert.True(first.TryGetProperty("isPublic", out _));
        Assert.True(first.TryGetProperty("userId", out _));
        Assert.True(first.TryGetProperty("brewingMethod", out _));
        Assert.True(first.TryGetProperty("coffee", out _));
    }

    [Fact]
    public async Task ShouldReturnRecipeDetailsForValidId()
    {
        var validId = 1;
        var response = await _client.GetAsync($"/api/Recipe/{validId}");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipe = document.RootElement;

        Assert.Equal(validId, recipe.GetProperty("id").GetInt32());
        Assert.True(recipe.TryGetProperty("title", out _));
        Assert.True(recipe.TryGetProperty("parameters", out _));
        Assert.True(recipe.TryGetProperty("steps", out _));
        Assert.True(recipe.TryGetProperty("isPublic", out _));
        Assert.True(recipe.TryGetProperty("userId", out _));
        Assert.True(recipe.TryGetProperty("brewingMethod", out _));
        Assert.True(recipe.TryGetProperty("coffee", out _));
    }

    [Fact]
    public async Task ShouldReturnNotFoundForNonExistingRecipeId()
    {
        var response = await _client.GetAsync("/api/Recipe/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public sealed class RecipeApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BrewDbContext>>();
            services.RemoveAll<BrewDbContext>();

            services.AddDbContext<BrewDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            context.Database.EnsureCreated();

            if (!context.Recipes.Any())
            {
                SeedRecipeData(context);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static void SeedRecipeData(BrewDbContext context)
    {
        var user = new AppUser
        {
            Id = 1,
            Login = "recipe.tester",
            Email = "recipe.tester@brewbase.local",
            PasswordHash = "test-hash",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var country = new Country
        {
            Id = 1,
            Name = "Test Country"
        };

        var region = new Region
        {
            Id = 1,
            Name = "Test Region",
            CountryId = country.Id
        };

        var roastery = new Roastery
        {
            Id = 1,
            Name = "Test Roastery"
        };

        var brewingMethod = new BrewingMethod
        {
            Id = 1,
            Name = "V60",
            Description = "Pour over"
        };

        var coffee = new Coffee
        {
            Id = 1,
            Name = "Test Coffee",
            IsVerified = true,
            RegionId = region.Id,
            RoasteryId = roastery.Id,
            CreatedByUserId = user.Id
        };

        var recipe = new Recipe
        {
            Id = 1,
            Title = "Test Recipe",
            Parameters = "{\"coffee_grams\":20,\"water_ml\":300,\"temperature\":94}",
            Steps = "1. Bloom\n2. Pour\n3. Finish",
            IsPublic = true,
            UserId = user.Id,
            BrewingMethodId = brewingMethod.Id,
            CoffeeId = coffee.Id
        };

        context.AppUsers.Add(user);
        context.Countries.Add(country);
        context.Regions.Add(region);
        context.Roasteries.Add(roastery);
        context.BrewingMethods.Add(brewingMethod);
        context.Coffees.Add(coffee);
        context.Recipes.Add(recipe);
        context.SaveChanges();
    }
}