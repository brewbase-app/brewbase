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
    public async Task ShouldReturnCoffeeRankingFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRankings.AddRange(
            new CoffeeRanking
            {
                CoffeeId = 2,
                Position = 1,
                AverageRating = 4.5,
                RatingCount = 10,
                RecipeUsedCount = 3,
                LikeCount = 0,
                RankingScore = 459,
                RefreshedAt = now
            },
            new CoffeeRanking
            {
                CoffeeId = 1,
                Position = 2,
                AverageRating = 4.2,
                RatingCount = 5,
                RecipeUsedCount = 1,
                LikeCount = 0,
                RankingScore = 431,
                RefreshedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);

        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(2, ranking[0].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Beta Coffee", ranking[0].GetProperty("name").GetString());
        Assert.Equal(4.5, ranking[0].GetProperty("averageRating").GetDouble());
        Assert.Equal(10, ranking[0].GetProperty("ratingCount").GetInt32());
        Assert.Equal(3, ranking[0].GetProperty("recipeUsedCount").GetInt32());

        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(1, ranking[1].GetProperty("coffeeId").GetInt32());
        Assert.Equal("Alpha Coffee", ranking[1].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldReturnExpectedCoffeeRankingFields()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRankings.Add(new CoffeeRanking
        {
            CoffeeId = 1,
            Position = 1,
            AverageRating = 4,
            RatingCount = 2,
            RecipeUsedCount = 1,
            LikeCount = 0,
            RankingScore = 405,
            RefreshedAt = now
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
    public async Task ShouldRespectCoffeeRankingLimitFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRankings.AddRange(
            new CoffeeRanking
            {
                CoffeeId = 1,
                Position = 1,
                AverageRating = 5,
                RatingCount = 5,
                RecipeUsedCount = 1,
                LikeCount = 0,
                RankingScore = 511,
                RefreshedAt = now
            },
            new CoffeeRanking
            {
                CoffeeId = 2,
                Position = 2,
                AverageRating = 4,
                RatingCount = 3,
                RecipeUsedCount = 1,
                LikeCount = 0,
                RankingScore = 407,
                RefreshedAt = now
            },
            new CoffeeRanking
            {
                CoffeeId = 3,
                Position = 3,
                AverageRating = 3,
                RatingCount = 1,
                RecipeUsedCount = 0,
                LikeCount = 0,
                RankingScore = 302,
                RefreshedAt = now
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
    public async Task ShouldIgnoreCoffeeRankingRowsWithZeroPosition()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.CoffeeRankings.AddRange(
            new CoffeeRanking
            {
                CoffeeId = 1,
                Position = 1,
                AverageRating = 4,
                RatingCount = 2,
                RecipeUsedCount = 1,
                LikeCount = 0,
                RankingScore = 405,
                RefreshedAt = now
            },
            new CoffeeRanking
            {
                CoffeeId = 2,
                Position = 0,
                AverageRating = 0,
                RatingCount = 0,
                RecipeUsedCount = 0,
                LikeCount = 0,
                RankingScore = 0,
                RefreshedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/coffees");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Single(ranking);
        Assert.Equal(1, ranking[0].GetProperty("coffeeId").GetInt32());
    }

    [Fact]
    public async Task ShouldReturnRecipeRankingFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var user = await CreateRankingUserAsync(context, "recipe-ranking-owner");
        var firstRecipe = await CreateRecipeAsync(context, user.Id, true);
        var secondRecipe = await CreateRecipeAsync(context, user.Id, true);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRankings.AddRange(
            new RecipeRanking
            {
                RecipeId = secondRecipe.Id,
                Position = 1,
                AverageRating = 4.8,
                RatingCount = 7,
                LikeCount = 0,
                SaveCount = 4,
                RankingScore = 498,
                RefreshedAt = now
            },
            new RecipeRanking
            {
                RecipeId = firstRecipe.Id,
                Position = 2,
                AverageRating = 4.2,
                RatingCount = 3,
                LikeCount = 0,
                SaveCount = 1,
                RankingScore = 427,
                RefreshedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/recipes");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);

        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(secondRecipe.Id, ranking[0].GetProperty("recipeId").GetInt32());
        Assert.Equal(4.8, ranking[0].GetProperty("averageRating").GetDouble());
        Assert.Equal(7, ranking[0].GetProperty("ratingCount").GetInt32());
        Assert.Equal(4, ranking[0].GetProperty("saveCount").GetInt32());

        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(firstRecipe.Id, ranking[1].GetProperty("recipeId").GetInt32());
    }

    [Fact]
    public async Task ShouldRespectRecipeRankingLimitFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var user = await CreateRankingUserAsync(context, "recipe-limit-owner");
        var firstRecipe = await CreateRecipeAsync(context, user.Id, true);
        var secondRecipe = await CreateRecipeAsync(context, user.Id, true);
        var thirdRecipe = await CreateRecipeAsync(context, user.Id, true);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRankings.AddRange(
            new RecipeRanking
            {
                RecipeId = firstRecipe.Id,
                Position = 1,
                AverageRating = 5,
                RatingCount = 5,
                LikeCount = 0,
                SaveCount = 3,
                RankingScore = 513,
                RefreshedAt = now
            },
            new RecipeRanking
            {
                RecipeId = secondRecipe.Id,
                Position = 2,
                AverageRating = 4,
                RatingCount = 4,
                LikeCount = 0,
                SaveCount = 2,
                RankingScore = 410,
                RefreshedAt = now
            },
            new RecipeRanking
            {
                RecipeId = thirdRecipe.Id,
                Position = 3,
                AverageRating = 3,
                RatingCount = 3,
                LikeCount = 0,
                SaveCount = 1,
                RankingScore = 307,
                RefreshedAt = now
            }
        );

        context.Articles.Add(new Article
        {
            UserId = user.Id,
            Title = $"Approved article {Guid.NewGuid()}",
            Content = "Article content",
            Module = "general",
            Status = "Approved",
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        });

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/recipes?limit=2");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);
        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(firstRecipe.Id, ranking[0].GetProperty("recipeId").GetInt32());
        Assert.Equal(secondRecipe.Id, ranking[1].GetProperty("recipeId").GetInt32());
    }

    [Fact]
    public async Task ShouldReturnUserRankingFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var firstUser = await CreateRankingUserAsync(context, "first-ranked-user");
        var secondUser = await CreateRankingUserAsync(context, "second-ranked-user");

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.UserRankings.AddRange(
            new UserRanking
            {
                UserId = secondUser.Id,
                Position = 1,
                ActivityScore = 120,
                RecipeCount = 2,
                LikeCount = 3,
                PublicRecipeCount = 2,
                CoffeeRatingCount = 4,
                RecipeRatingCount = 3,
                QuickNoteCount = 1,
                CuppingSessionCount = 5,
                CuppingSessionCoffeeCount = 6,
                FollowersCount = 2,
                ReceivedRecipeFavoriteCount = 3,
                PublishedArticleCount = 1,
                RefreshedAt = now
            },
            new UserRanking
            {
                UserId = firstUser.Id,
                Position = 2,
                ActivityScore = 80,
                RecipeCount = 1,
                LikeCount = 1,
                PublicRecipeCount = 1,
                CoffeeRatingCount = 2,
                RecipeRatingCount = 1,
                QuickNoteCount = 0,
                CuppingSessionCount = 3,
                CuppingSessionCoffeeCount = 4,
                FollowersCount = 1,
                ReceivedRecipeFavoriteCount = 1,
                PublishedArticleCount = 0,
                RefreshedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/users");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);

        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(secondUser.Id, ranking[0].GetProperty("userId").GetInt32());
        Assert.Equal(120, ranking[0].GetProperty("activityScore").GetInt32());
        Assert.Equal(2, ranking[0].GetProperty("publicRecipeCount").GetInt32());
        Assert.Equal(4, ranking[0].GetProperty("coffeeRatingCount").GetInt32());
        Assert.Equal(3, ranking[0].GetProperty("recipeRatingCount").GetInt32());

        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(firstUser.Id, ranking[1].GetProperty("userId").GetInt32());
    }

    [Fact]
    public async Task ShouldRespectUserRankingLimitFromRankingTable()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var firstUser = await CreateRankingUserAsync(context, "limit-first-user");
        var secondUser = await CreateRankingUserAsync(context, "limit-second-user");
        var thirdUser = await CreateRankingUserAsync(context, "limit-third-user");

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.UserRankings.AddRange(
            new UserRanking
            {
                UserId = firstUser.Id,
                Position = 1,
                ActivityScore = 100,
                RecipeCount = 5,
                LikeCount = 2,
                PublicRecipeCount = 5,
                CoffeeRatingCount = 1,
                RecipeRatingCount = 1,
                QuickNoteCount = 0,
                CuppingSessionCount = 0,
                CuppingSessionCoffeeCount = 0,
                FollowersCount = 2,
                ReceivedRecipeFavoriteCount = 2,
                PublishedArticleCount = 0,
                RefreshedAt = now
            },
            new UserRanking
            {
                UserId = secondUser.Id,
                Position = 2,
                ActivityScore = 90,
                RecipeCount = 4,
                LikeCount = 1,
                PublicRecipeCount = 4,
                CoffeeRatingCount = 1,
                RecipeRatingCount = 1,
                QuickNoteCount = 0,
                CuppingSessionCount = 0,
                CuppingSessionCoffeeCount = 0,
                FollowersCount = 1,
                ReceivedRecipeFavoriteCount = 1,
                PublishedArticleCount = 0,
                RefreshedAt = now
            },
            new UserRanking
            {
                UserId = thirdUser.Id,
                Position = 3,
                ActivityScore = 80,
                RecipeCount = 3,
                LikeCount = 0,
                PublicRecipeCount = 3,
                CoffeeRatingCount = 1,
                RecipeRatingCount = 1,
                QuickNoteCount = 0,
                CuppingSessionCount = 0,
                CuppingSessionCoffeeCount = 0,
                FollowersCount = 0,
                ReceivedRecipeFavoriteCount = 0,
                PublishedArticleCount = 0,
                RefreshedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/users?limit=2");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);
        Assert.Equal(firstUser.Id, ranking[0].GetProperty("userId").GetInt32());
        Assert.Equal(secondUser.Id, ranking[1].GetProperty("userId").GetInt32());
    }

    [Fact]
    public async Task ShouldReturnNoContentWhenRefreshingRankings()
    {
        var response = await _client.PostAsync("/api/Ranking/refresh", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task<AppUser> CreateRankingUserAsync(
        BrewDbContext context,
        string loginPrefix,
        bool isBlocked = false)
    {
        var uniqueValue = Guid.NewGuid().ToString("N");

        var user = new AppUser
        {
            Login = $"{loginPrefix}-{uniqueValue}",
            Email = $"{loginPrefix}-{uniqueValue}@brewbase.local",
            PasswordHash = "test-hash",
            Role = "User",
            IsBlocked = isBlocked,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private static async Task<Recipe> CreateRecipeAsync(
        BrewDbContext context,
        int userId,
        bool isPublic)
    {
        var recipe = new Recipe
        {
            Title = $"Ranking recipe {Guid.NewGuid()}",
            Parameters = "{}",
            Steps = "step",
            IsPublic = isPublic,
            UserId = userId,
            CoffeeId = 1,
            BrewingMethodId = 1
        };

        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();

        return recipe;
    }

    private static async Task<JsonElement> ParseResponseRootAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}