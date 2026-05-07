using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.AspNetCore.Hosting;
using brewbase.server.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace brewbase.server.Tests;

public class RecipeEndpointsTests : IDisposable
{
    private const int User1 = 1;
    private const int User2 = 2;
    private const int Admin = 3;

    private readonly RecipeApiFactory _factory;
    private readonly HttpClient _client;

    public RecipeEndpointsTests()
    {
        _factory = new RecipeApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Unauthenticated_GetAll_ReturnsUnauthorized()
    {
        var response = await SendRecipeGetAsync("/api/Recipe");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_GetById_ReturnsUnauthorized()
    {
        var response = await SendRecipeGetAsync("/api/Recipe/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnListOfRecipes()
    {
        var response = await SendRecipeGetAsync("/api/Recipe", devUserId: User1);
        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Equal(2, recipeList.Count);
        Assert.All(recipeList, r => Assert.True(r.GetProperty("isPublic").GetBoolean()));

        var first = recipeList.First();
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
        var response = await SendRecipeGetAsync($"/api/Recipe/{validId}", devUserId: User1);

        response.EnsureSuccessStatusCode();

        var recipe = await ParseResponseRootAsync(response);

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
        var response = await SendRecipeGetAsync("/api/Recipe/999999", devUserId: User1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldFilterRecipesByCoffeeId()
    {
        // user 1: private recipe of user 2 on same coffee is hidden
        var response = await SendRecipeGetAsync("/api/Recipe?coffeeId=1", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Single(recipeList);
        Assert.Equal("Alpha Recipe", recipeList.First().GetProperty("title").GetString());
        Assert.All(recipeList, recipe =>
        {
            Assert.Equal("Alpha Coffee", recipe.GetProperty("coffee").GetString());
        });
    }

    [Fact]
    public async Task Unauthenticated_FilterByUserId_ReturnsUnauthorized()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?userId=2");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User1_FilterByUserId_OtherOwner_DoesNotRevealPrivateRecipes()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?userId=2", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Empty(recipeList);
    }

    [Fact]
    public async Task User2_FilterByUserId_SeesOwnPrivateRecipe()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?userId=2", devUserId: User2);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Single(recipeList);
        Assert.Equal("Zulu Recipe", recipeList.First().GetProperty("title").GetString());
    }

    [Fact]
    public async Task User2_FilterByUserId_OtherOwner_OnlyShowsTheirPublicRecipes()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?userId=1", devUserId: User2);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Equal(2, recipeList.Count);
        Assert.All(recipeList, r =>
        {
            Assert.Equal(1, r.GetProperty("userId").GetInt32());
            Assert.True(r.GetProperty("isPublic").GetBoolean());
        });
    }

    [Fact]
    public async Task ShouldFilterRecipesByBrewingMethodId()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?brewingMethodId=2", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Single(recipeList);
        Assert.Equal("Beta Recipe", recipeList.First().GetProperty("title").GetString());
    }

    [Fact(Skip = "SQLite does not support EF.Functions.ILike; verified separately on PostgreSQL")]
    public async Task ShouldSearchRecipesByTitle()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?search=beta", devUserId: User1);
        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        Assert.Single(recipeList);
        Assert.Equal("Beta Recipe", recipeList.First().GetProperty("title").GetString());
    }

    [Fact]
    public async Task ShouldSortRecipesByTitleAscending()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?sortBy=title&sortOrder=asc", devUserId: User1);
        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        var titles = recipeList
            .Select(r => r.GetProperty("title").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Alpha Recipe", "Beta Recipe" }, titles);
    }

    [Fact]
    public async Task ShouldSortRecipesByTitleDescending()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?sortBy=title&sortOrder=desc", devUserId: User1);
        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        var titles = recipeList
            .Select(r => r.GetProperty("title").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Beta Recipe", "Alpha Recipe" }, titles);
    }

    [Fact]
    public async Task ShouldPaginateRecipes()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?page=2&pageSize=1", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.Single(recipeList);
        Assert.Equal("Beta Recipe", recipeList.First().GetProperty("title").GetString());
    }

    [Fact]
    public async Task ShouldHandleInvalidPaginationValuesSafely()
    {
        var response = await SendRecipeGetAsync("/api/Recipe?page=-1&pageSize=-5", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var recipeList = root.EnumerateArray().ToList();
        Assert.True(recipeList.Count > 0);
    }

    [Fact]
    public async Task User1_GetAll_SeesPublicRecipesButNotOtherUsersPrivate()
    {
        var response = await SendRecipeGetAsync("/api/Recipe", devUserId: User1);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        Assert.Equal(2, recipeList.Count);
        var titles = recipeList.Select(r => r.GetProperty("title").GetString()).OrderBy(t => t).ToList();
        Assert.Equal(new List<string?> { "Alpha Recipe", "Beta Recipe" }, titles);
    }

    [Fact]
    public async Task User2_GetAll_SeesPublicRecipesAndOwnPrivateRecipe()
    {
        var response = await SendRecipeGetAsync("/api/Recipe", devUserId: User2);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        Assert.Equal(3, recipeList.Count);
        var titles = recipeList.Select(r => r.GetProperty("title").GetString()).OrderBy(t => t).ToList();
        Assert.Equal(new List<string?> { "Alpha Recipe", "Beta Recipe", "Zulu Recipe" }, titles);
    }

    [Fact]
    public async Task Admin_GetAll_SeesOnlyPublicAndOwnPrivate_NotOthersPrivate()
    {
        var response = await SendRecipeGetAsync("/api/Recipe", devUserId: Admin);
        response.EnsureSuccessStatusCode();
        var root = await ParseResponseRootAsync(response);

        var recipeList = root.EnumerateArray().ToList();
        Assert.Equal(2, recipeList.Count);
        var titles = recipeList.Select(r => r.GetProperty("title").GetString()).OrderBy(t => t).ToList();
        Assert.Equal(new List<string?> { "Alpha Recipe", "Beta Recipe" }, titles);
    }

    [Fact]
    public async Task Admin_GetById_OtherUsersPrivateRecipe_ReturnsNotFound()
    {
        var response = await SendRecipeGetAsync("/api/Recipe/3", devUserId: Admin);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_GetById_OtherUsersPrivateRecipe_ReturnsNotFound()
    {
        var response = await SendRecipeGetAsync("/api/Recipe/3", devUserId: User1);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User2_GetById_OwnPrivateRecipe_ReturnsOk()
    {
        var response = await SendRecipeGetAsync("/api/Recipe/3", devUserId: User2);
        response.EnsureSuccessStatusCode();
        var recipe = await ParseResponseRootAsync(response);
        Assert.Equal(3, recipe.GetProperty("id").GetInt32());
        Assert.Equal("Zulu Recipe", recipe.GetProperty("title").GetString());
        Assert.False(recipe.GetProperty("isPublic").GetBoolean());
    }

    [Fact]
    public async Task User1_Put_OtherUsersPrivateRecipe_ReturnsNotFound()
    {
        var body = """
            {"title":"X","parameters":{},"steps":"y","isPublic":false,"coffeeId":1,"brewingMethodId":1}
            """;
        var response = await SendRecipeWriteAsync(HttpMethod.Put, "/api/Recipe/3", devUserId: User1, body);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Delete_OtherUsersPrivateRecipe_ReturnsNotFound()
    {
        var response = await SendRecipeWriteAsync(HttpMethod.Delete, "/api/Recipe/3", devUserId: User1);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Put_NonExistingRecipe_ReturnsNotFound()
    {
        var body = """
            {"title":"X","parameters":{},"steps":"y","isPublic":true,"coffeeId":1,"brewingMethodId":1}
            """;
        var response = await SendRecipeWriteAsync(HttpMethod.Put, "/api/Recipe/999999", devUserId: User1, body);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Delete_NonExistingRecipe_ReturnsNotFound()
    {
        var response = await SendRecipeWriteAsync(HttpMethod.Delete, "/api/Recipe/999999", devUserId: User1);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User2_Put_OthersPublicRecipe_ReturnsForbidden()
    {
        var body = """
            {"title":"X","parameters":{},"steps":"y","isPublic":true,"coffeeId":1,"brewingMethodId":1}
            """;
        var response = await SendRecipeWriteAsync(HttpMethod.Put, "/api/Recipe/1", devUserId: User2, body);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User1_CreateRecipe_ReturnsCreated()
    {
        var body = """
            {"title":"Created Via Test","parameters":{},"steps":"step","isPublic":true,"coffeeId":1,"brewingMethodId":1}
            """;
        var response = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe", devUserId: User1, body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var recipe = await ParseResponseRootAsync(response);
        Assert.True(recipe.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Created Via Test", recipe.GetProperty("title").GetString());
    }
    
    [Fact]
    public async Task User2_GetPrivateRecipeWithoutRatings_ReturnsNullAverageRatingAndZeroRatingCount()
    {
        var response = await SendRecipeGetAsync("/api/Recipe/3", devUserId: User2);

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        Assert.Equal(JsonValueKind.Null, root.GetProperty("averageRating").ValueKind);
        Assert.Equal(0, root.GetProperty("ratingCount").GetInt32());
    }

    [Fact]
    public async Task User1_GetRecipeWithRatings_ReturnsAverageRatingAndRatingCount()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var recipe = new Recipe
        {
            Title = $"Rated Recipe {Guid.NewGuid()}",
            Parameters = "{}",
            Steps = "step",
            IsPublic = true,
            UserId = User1,
            CoffeeId = 1,
            BrewingMethodId = 1
        };

        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRatings.AddRange(
            new RecipeRating
            {
                RecipeId = recipe.Id,
                UserId = User1,
                Value = 3,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = recipe.Id,
                UserId = User2,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await SendRecipeGetAsync($"/api/Recipe/{recipe.Id}", devUserId: User1);

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);

        Assert.Equal(4, root.GetProperty("averageRating").GetDouble());
        Assert.Equal(2, root.GetProperty("ratingCount").GetInt32());
    }
    
    [Fact]
    public async Task User1_RateExistingRecipe_ReturnsNoContent()
    {
        var body = """
            {"value":4}
            """;

        var response = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe/1/rating", User1, body);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var rating = context.RecipeRatings.Single(r => r.RecipeId == 1 && r.UserId == User1);

        Assert.Equal(4, rating.Value);
    }

    [Fact]
    public async Task User1_RateExistingRecipeTwice_UpdatesPreviousRating()
    {
        var firstBody = """
            {"value":2}
            """;

        var secondBody = """
            {"value":5}
            """;

        var firstResponse = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe/2/rating", User1, firstBody);
        var secondResponse = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe/2/rating", User1, secondBody);

        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var ratings = context.RecipeRatings
            .Where(r => r.RecipeId == 2 && r.UserId == User1)
            .ToList();

        Assert.Single(ratings);
        Assert.Equal(5, ratings[0].Value);
    }

    [Fact]
    public async Task User1_RateNonExistingRecipe_ReturnsNotFound()
    {
        var body = """
            {"value":4}
            """;

        var response = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe/999999/rating", User1, body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_RateRecipeWithInvalidValue_ReturnsBadRequest()
    {
        var body = """
            {"value":6}
            """;

        var response = await SendRecipeWriteAsync(HttpMethod.Post, "/api/Recipe/1/rating", User1, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendRecipeWriteAsync(HttpMethod method, string path, int devUserId, string? jsonBody = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, devUserId.ToString());
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendRecipeGetAsync(string path, int? devUserId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (devUserId is int uid)
        {
            request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, uid.ToString());
        }

        return await _client.SendAsync(request);
    }

    private static async Task<JsonElement> ParseResponseRootAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}

public sealed class RecipeApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _sqliteConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _sqliteConnection = connection;

        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "TEST_SECRET_KEY_12345678901234567890" },
                { "Jwt:Issuer", "test" },
                { "Jwt:Audience", "test" },
                // Override appsettings.json DevUser:UserId so requests without X-Dev-User-Id are unauthenticated.
                { "DevUser:UserId", "0" }
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });

            services.RemoveAll<DbContextOptions<BrewDbContext>>();
            services.RemoveAll<BrewDbContext>();

            services.AddDbContext<BrewDbContext>(options =>
            {
                options.UseSqlite(connection);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        context.Database.EnsureCreated();
        if (!context.Recipes.Any())
        {
            SeedRecipeData(context);
        }

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _sqliteConnection?.Dispose();
            _sqliteConnection = null;
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

        var user3Admin = new AppUser
        {
            Id = 3,
            Login = "recipe.tester.admin",
            Email = "recipe.tester.admin@brewbase.local",
            PasswordHash = "test-hash",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        context.AppUsers.AddRange(user1, user2, user3Admin);
        context.Countries.Add(country);
        context.Regions.Add(region);
        context.Roasteries.Add(roastery);
        context.BrewingMethods.AddRange(brewingMethod1, brewingMethod2);
        context.Coffees.AddRange(coffee1, coffee2);
        context.Recipes.AddRange(recipe1, recipe2, recipe3);
        context.SaveChanges();
    }
}
