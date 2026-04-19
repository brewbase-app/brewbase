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
    [Fact]
    public async Task ShouldFilterRecipesByCoffeeId()
    {
        var response = await _client.GetAsync("/api/Recipe?coffeeId=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Equal(2, recipes.GetArrayLength());
        Assert.All(recipes.EnumerateArray(), recipe =>
        {
            Assert.Equal("Alpha Coffee", recipe.GetProperty("coffee").GetString());
        });
    }

    [Fact]
    public async Task ShouldFilterRecipesByUserId()
    {
        var response = await _client.GetAsync("/api/Recipe?userId=2");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Single(recipes.EnumerateArray());
        Assert.Equal("Zulu Recipe", recipes[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ShouldFilterRecipesByBrewingMethodId()
    {
        var response = await _client.GetAsync("/api/Recipe?brewingMethodId=2");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Single(recipes.EnumerateArray());
        Assert.Equal("Beta Recipe", recipes[0].GetProperty("title").GetString());
    }

    [Fact(Skip = "Temporary disabled: EF.Functions.ILike is not translated by SQLite in integration tests.")]
    public async Task ShouldSearchRecipesByTitle()
    {
        var response = await _client.GetAsync("/api/Recipe?search=beta");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Single(recipes.EnumerateArray());
        Assert.Equal("Beta Recipe", recipes[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task ShouldSortRecipesByTitleAscending()
    {
        var response = await _client.GetAsync("/api/Recipe?sortBy=title&sortOrder=asc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        var titles = recipes.EnumerateArray()
            .Select(r => r.GetProperty("title").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Alpha Recipe", "Beta Recipe", "Zulu Recipe" }, titles);
    }

    [Fact]
    public async Task ShouldSortRecipesByTitleDescending()
    {
        var response = await _client.GetAsync("/api/Recipe?sortBy=title&sortOrder=desc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        var titles = recipes.EnumerateArray()
            .Select(r => r.GetProperty("title").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Zulu Recipe", "Beta Recipe", "Alpha Recipe" }, titles);
    }

    [Fact]
    public async Task ShouldPaginateRecipes()
    {
        var response = await _client.GetAsync("/api/Recipe?page=2&pageSize=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var recipes = document.RootElement;

        Assert.Single(recipes.EnumerateArray());
        Assert.Equal("Beta Recipe", recipes[0].GetProperty("title").GetString());
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
    var user1 = new AppUser
    {
        Id = 1,
        Login = "recipe.tester.one",
        Email = "recipe.tester.one@brewbase.local",
        PasswordHash = "test-hash",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    var user2 = new AppUser
    {
        Id = 2,
        Login = "recipe.tester.two",
        Email = "recipe.tester.two@brewbase.local",
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

    var brewingMethod1 = new BrewingMethod
    {
        Id = 1,
        Name = "V60",
        Description = "Pour over"
    };

    var brewingMethod2 = new BrewingMethod
    {
        Id = 2,
        Name = "AeroPress",
        Description = "Pressure brewing"
    };

    var coffee1 = new Coffee
    {
        Id = 1,
        Name = "Alpha Coffee",
        IsVerified = true,
        RegionId = region.Id,
        RoasteryId = roastery.Id,
        CreatedByUserId = user1.Id
    };

    var coffee2 = new Coffee
    {
        Id = 2,
        Name = "Beta Coffee",
        IsVerified = true,
        RegionId = region.Id,
        RoasteryId = roastery.Id,
        CreatedByUserId = user1.Id
    };

    var recipe1 = new Recipe
    {
        Id = 1,
        Title = "Alpha Recipe",
        Parameters = "{\"coffee_grams\":20,\"water_ml\":300,\"temperature\":94}",
        Steps = "1. Bloom\n2. Pour\n3. Finish",
        IsPublic = true,
        UserId = user1.Id,
        BrewingMethodId = brewingMethod1.Id,
        CoffeeId = coffee1.Id
    };

    var recipe2 = new Recipe
    {
        Id = 2,
        Title = "Beta Recipe",
        Parameters = "{\"coffee_grams\":18,\"water_ml\":250,\"temperature\":92}",
        Steps = "1. Stir\n2. Press\n3. Serve",
        IsPublic = true,
        UserId = user1.Id,
        BrewingMethodId = brewingMethod2.Id,
        CoffeeId = coffee2.Id
    };

    var recipe3 = new Recipe
    {
        Id = 3,
        Title = "Zulu Recipe",
        Parameters = "{\"coffee_grams\":22,\"water_ml\":320,\"temperature\":95}",
        Steps = "1. Rinse\n2. Pour\n3. Drawdown",
        IsPublic = false,
        UserId = user2.Id,
        BrewingMethodId = brewingMethod1.Id,
        CoffeeId = coffee1.Id
    };

    context.AppUsers.AddRange(user1, user2);
    context.Countries.Add(country);
    context.Regions.Add(region);
    context.Roasteries.Add(roastery);
    context.BrewingMethods.AddRange(brewingMethod1, brewingMethod2);
    context.Coffees.AddRange(coffee1, coffee2);
    context.Recipes.AddRange(recipe1, recipe2, recipe3);
    context.SaveChanges();
}
}