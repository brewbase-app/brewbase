using System.Net;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class RankingEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public RankingEndpointsTests()
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
    public async Task ShouldReturnCoffeeRankingOrderedByAverageRatingAndRatingCount()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRatings.AddRange(
            new CoffeeRating
            {
                CoffeeId = 1,
                UserId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 2,
                UserId = 1,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 2,
                UserId = 2,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 3,
                UserId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 3,
                UserId = 2,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(3, ranking.Count);

        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(3, ranking[0].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Zulu Coffee", ranking[0].GetProperty("name").GetString());
        Assert.Equal(5, ranking[0].GetProperty("averageRating").GetDouble());
        Assert.Equal(2, ranking[0].GetProperty("ratingCount").GetInt32());

        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(1, ranking[1].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Alpha Coffee", ranking[1].GetProperty("name").GetString());
        Assert.Equal(5, ranking[1].GetProperty("averageRating").GetDouble());
        Assert.Equal(1, ranking[1].GetProperty("ratingCount").GetInt32());

        Assert.Equal(3, ranking[2].GetProperty("position").GetInt32());
        Assert.Equal(2, ranking[2].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Beta Coffee", ranking[2].GetProperty("name").GetString());
        Assert.Equal(4.5, ranking[2].GetProperty("averageRating").GetDouble());
        Assert.Equal(2, ranking[2].GetProperty("ratingCount").GetInt32());
    }

    [Fact]
    public async Task ShouldReturnExpectedCoffeeRankingFields()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRatings.Add(new CoffeeRating
        {
            CoffeeId = 1,
            UserId = 1,
            Value = 4,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var first = root.EnumerateArray().First();

        Assert.True(first.TryGetProperty("position", out _));
        Assert.True(first.TryGetProperty("coffeeId", out _));
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("region", out _));
        Assert.True(first.TryGetProperty("roastery", out _));
        Assert.True(first.TryGetProperty("processingMethod", out _));
        Assert.True(first.TryGetProperty("variety", out _));
        Assert.True(first.TryGetProperty("averageRating", out _));
        Assert.True(first.TryGetProperty("ratingCount", out _));
        Assert.True(first.TryGetProperty("recipeUsedCount", out _));
    }

    [Fact]
    public async Task ShouldExcludeCoffeesWithoutRatingsFromRanking()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRatings.Add(new CoffeeRating
        {
            CoffeeId = 1,
            UserId = 1,
            Value = 5,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Single(ranking);
        Assert.Equal(1, ranking[0].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Alpha Coffee", ranking[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldRespectCoffeeRankingLimit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRatings.AddRange(
            new CoffeeRating
            {
                CoffeeId = 1,
                UserId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 2,
                UserId = 1,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 3,
                UserId = 1,
                Value = 3,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees?limit=2");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);
        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
    }

    [Fact]
    public async Task ShouldUseRecipeUsedCountAsTieBreaker()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        await EnsureTestUsersExistAsync(context);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRatings.AddRange(
            new CoffeeRating
            {
                CoffeeId = 1,
                UserId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                CoffeeId = 2,
                UserId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        context.Recipes.AddRange(
            new Recipe
            {
                Title = $"Alpha Recipe {Guid.NewGuid()}",
                Parameters = "{}",
                Steps = "step",
                IsPublic = true,
                UserId = 1,
                CoffeeId = 1,
                BrewingMethodId = 1
            },
            new Recipe
            {
                Title = $"Beta Recipe 1 {Guid.NewGuid()}",
                Parameters = "{}",
                Steps = "step",
                IsPublic = true,
                UserId = 1,
                CoffeeId = 2,
                BrewingMethodId = 1
            },
            new Recipe
            {
                Title = $"Beta Recipe 2 {Guid.NewGuid()}",
                Parameters = "{}",
                Steps = "step",
                IsPublic = true,
                UserId = 1,
                CoffeeId = 2,
                BrewingMethodId = 1
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);
        Assert.Equal(2, ranking[0].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Beta Coffee", ranking[0].GetProperty("name").GetString());
        Assert.Equal(2, ranking[0].GetProperty("recipeUsedCount").GetInt32());

        Assert.Equal(1, ranking[1].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Alpha Coffee", ranking[1].GetProperty("name").GetString());
        Assert.Equal(1, ranking[1].GetProperty("recipeUsedCount").GetInt32());
    }

    private static async Task EnsureTestUsersExistAsync(BrewDbContext context)
    {
        var secondUserExists = await context.AppUsers.AnyAsync(user => user.Id == 2);

        if (!secondUserExists)
        {
            context.AppUsers.Add(new AppUser
            {
                Id = 2,
                Login = "ranking.user.two",
                Email = "ranking.user.two@brewbase.local",
                PasswordHash = "test-hash",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            });
        }

        var thirdUserExists = await context.AppUsers.AnyAsync(user => user.Id == 3);

        if (!thirdUserExists)
        {
            context.AppUsers.Add(new AppUser
            {
                Id = 3,
                Login = "ranking.user.three",
                Email = "ranking.user.three@brewbase.local",
                PasswordHash = "test-hash",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<JsonElement> ParseResponseRootAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}