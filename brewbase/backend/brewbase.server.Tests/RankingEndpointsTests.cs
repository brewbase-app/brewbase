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
    
    [Fact]
    public async Task ShouldCalculateUserActivityScoreCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var user = await CreateRankingUserAsync(context, "score-user");
        var followerOne = await CreateRankingUserAsync(context, "follower-one");
        var followerTwo = await CreateRankingUserAsync(context, "follower-two");

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var userRecipe = await CreateRecipeAsync(context, user.Id, true);
        var ratedRecipe = await CreateRecipeAsync(context, followerOne.Id, true);

        context.CoffeeRatings.AddRange(
            new CoffeeRating
            {
                UserId = user.Id,
                CoffeeId = 1,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new CoffeeRating
            {
                UserId = user.Id,
                CoffeeId = 2,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        context.RecipeRatings.Add(new RecipeRating
        {
            UserId = user.Id,
            RecipeId = ratedRecipe.Id,
            Value = 5,
            CreatedAt = now,
            UpdatedAt = now
        });

        context.QuickNotes.Add(new QuickNote
        {
            UserId = user.Id,
            Content = "Ranking test note",
            CreatedAt = now,
            UpdatedAt = now
        });

        var session = new CuppingSession
        {
            UserId = user.Id,
            Name = $"Ranking session {Guid.NewGuid()}",
            Description = "Ranking session description",
            CreatedAt = now
        };

        context.CuppingSessions.Add(session);
        await context.SaveChangesAsync();

        context.CuppingSessionCoffees.AddRange(
            new CuppingSessionCoffee
            {
                CuppingSessionId = session.Id,
                CoffeeId = 1,
                CreatedAt = now
            },
            new CuppingSessionCoffee
            {
                CuppingSessionId = session.Id,
                CoffeeId = 2,
                CreatedAt = now
            }
        );

        context.Follows.AddRange(
            new Follow
            {
                FollowerId = followerOne.Id,
                FollowedId = user.Id,
                CreatedAt = now
            },
            new Follow
            {
                FollowerId = followerTwo.Id,
                FollowedId = user.Id,
                CreatedAt = now
            }
        );

        context.UserRecipeFavorites.AddRange(
            new UserRecipeFavorite
            {
                UserId = followerOne.Id,
                RecipeId = userRecipe.Id,
                CreatedAt = now
            },
            new UserRecipeFavorite
            {
                UserId = followerTwo.Id,
                RecipeId = userRecipe.Id,
                CreatedAt = now
            }
        );

        context.Articles.Add(new Article
        {
            UserId = user.Id,
            Title = $"Approved article {Guid.NewGuid()}",
            Content = "Article content",
            Status = "APPROVED",
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        });

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/users");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();
        var rankedUser = ranking.Single(item => item.GetProperty("userId").GetInt32() == user.Id);

        Assert.Equal(63, rankedUser.GetProperty("activityScore").GetInt32());
        Assert.Equal(1, rankedUser.GetProperty("publicRecipeCount").GetInt32());
        Assert.Equal(2, rankedUser.GetProperty("coffeeRatingCount").GetInt32());
        Assert.Equal(1, rankedUser.GetProperty("recipeRatingCount").GetInt32());
        Assert.Equal(1, rankedUser.GetProperty("quickNoteCount").GetInt32());
        Assert.Equal(1, rankedUser.GetProperty("cuppingSessionCount").GetInt32());
        Assert.Equal(2, rankedUser.GetProperty("cuppingSessionCoffeeCount").GetInt32());
        Assert.Equal(2, rankedUser.GetProperty("followersCount").GetInt32());
        Assert.Equal(2, rankedUser.GetProperty("receivedRecipeFavoriteCount").GetInt32());
        Assert.Equal(1, rankedUser.GetProperty("publishedArticleCount").GetInt32());
    }

    [Fact]
    public async Task ShouldOrderUsersByActivityScoreAndExcludeInactiveAndBlockedUsers()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var highScoreUser = await CreateRankingUserAsync(context, "high-score-user");
        var lowScoreUser = await CreateRankingUserAsync(context, "low-score-user");
        var inactiveUser = await CreateRankingUserAsync(context, "inactive-user");
        var blockedUser = await CreateRankingUserAsync(context, "blocked-user", true);

        await AddPublicRecipesAsync(context, highScoreUser.Id, 5);
        await AddPublicRecipesAsync(context, lowScoreUser.Id, 1);
        await AddPublicRecipesAsync(context, blockedUser.Id, 10);

        var response = await _client.GetAsync("/api/Ranking/users");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        var highScoreIndex = ranking.FindIndex(item => item.GetProperty("userId").GetInt32() == highScoreUser.Id);
        var lowScoreIndex = ranking.FindIndex(item => item.GetProperty("userId").GetInt32() == lowScoreUser.Id);

        Assert.True(highScoreIndex >= 0);
        Assert.True(lowScoreIndex >= 0);
        Assert.True(highScoreIndex < lowScoreIndex);

        Assert.DoesNotContain(ranking, item => item.GetProperty("userId").GetInt32() == inactiveUser.Id);
        Assert.DoesNotContain(ranking, item => item.GetProperty("userId").GetInt32() == blockedUser.Id);
    }

    [Fact]
    public async Task ShouldRespectUserRankingLimit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var firstUser = await CreateRankingUserAsync(context, "limit-first-user");
        var secondUser = await CreateRankingUserAsync(context, "limit-second-user");
        var thirdUser = await CreateRankingUserAsync(context, "limit-third-user");

        await AddPublicRecipesAsync(context, firstUser.Id, 10);
        await AddPublicRecipesAsync(context, secondUser.Id, 9);
        await AddPublicRecipesAsync(context, thirdUser.Id, 8);

        var response = await _client.GetAsync("/api/Ranking/users?limit=2");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Equal(2, ranking.Count);
        Assert.Equal(1, ranking[0].GetProperty("position").GetInt32());
        Assert.Equal(2, ranking[1].GetProperty("position").GetInt32());
        Assert.Equal(firstUser.Id, ranking[0].GetProperty("userId").GetInt32());
        Assert.Equal(secondUser.Id, ranking[1].GetProperty("userId").GetInt32());
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

    private static async Task AddPublicRecipesAsync(
        BrewDbContext context,
        int userId,
        int count)
    {
        for (var index = 0; index < count; index++)
        {
            context.Recipes.Add(new Recipe
            {
                Title = $"Ranking recipe {Guid.NewGuid()}",
                Parameters = "{}",
                Steps = "step",
                IsPublic = true,
                UserId = userId,
                CoffeeId = 1,
                BrewingMethodId = 1
            });
        }

        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task ShouldReturnRecipeRankingOrderedByAverageRatingAndRatingCount()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var owner = await CreateRankingUserAsync(context, "recipe-ranking-owner");
        var firstRater = await CreateRankingUserAsync(context, "recipe-ranking-rater-one");
        var secondRater = await CreateRankingUserAsync(context, "recipe-ranking-rater-two");

        var firstRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var secondRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var thirdRecipe = await CreateRecipeAsync(context, owner.Id, true);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRatings.AddRange(
            new RecipeRating
            {
                RecipeId = firstRecipe.Id,
                UserId = firstRater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = secondRecipe.Id,
                UserId = firstRater.Id,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = secondRecipe.Id,
                UserId = secondRater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = thirdRecipe.Id,
                UserId = firstRater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = thirdRecipe.Id,
                UserId = secondRater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/recipes");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        var thirdRankedRecipe = ranking.Single(item => item.GetProperty("recipeId").GetInt32() == thirdRecipe.Id);
        var firstRankedRecipe = ranking.Single(item => item.GetProperty("recipeId").GetInt32() == firstRecipe.Id);
        var secondRankedRecipe = ranking.Single(item => item.GetProperty("recipeId").GetInt32() == secondRecipe.Id);

        Assert.True(thirdRankedRecipe.GetProperty("position").GetInt32() < firstRankedRecipe.GetProperty("position").GetInt32());
        Assert.True(firstRankedRecipe.GetProperty("position").GetInt32() < secondRankedRecipe.GetProperty("position").GetInt32());

        Assert.Equal(5, thirdRankedRecipe.GetProperty("averageRating").GetDouble());
        Assert.Equal(2, thirdRankedRecipe.GetProperty("ratingCount").GetInt32());

        Assert.Equal(5, firstRankedRecipe.GetProperty("averageRating").GetDouble());
        Assert.Equal(1, firstRankedRecipe.GetProperty("ratingCount").GetInt32());

        Assert.Equal(4.5, secondRankedRecipe.GetProperty("averageRating").GetDouble());
        Assert.Equal(2, secondRankedRecipe.GetProperty("ratingCount").GetInt32());
    }

    [Fact]
    public async Task ShouldExcludePrivateRecipesFromRecipeRanking()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var owner = await CreateRankingUserAsync(context, "private-recipe-owner");
        var rater = await CreateRankingUserAsync(context, "private-recipe-rater");

        var publicRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var privateRecipe = await CreateRecipeAsync(context, owner.Id, false);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRatings.AddRange(
            new RecipeRating
            {
                RecipeId = publicRecipe.Id,
                UserId = rater.Id,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = privateRecipe.Id,
                UserId = rater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/recipes");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        Assert.Contains(ranking, item => item.GetProperty("recipeId").GetInt32() == publicRecipe.Id);
        Assert.DoesNotContain(ranking, item => item.GetProperty("recipeId").GetInt32() == privateRecipe.Id);
    }

    [Fact]
    public async Task ShouldRespectRecipeRankingLimit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var owner = await CreateRankingUserAsync(context, "recipe-limit-owner");
        var rater = await CreateRankingUserAsync(context, "recipe-limit-rater");

        var firstRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var secondRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var thirdRecipe = await CreateRecipeAsync(context, owner.Id, true);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRatings.AddRange(
            new RecipeRating
            {
                RecipeId = firstRecipe.Id,
                UserId = rater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = secondRecipe.Id,
                UserId = rater.Id,
                Value = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = thirdRecipe.Id,
                UserId = rater.Id,
                Value = 3,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

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
    public async Task ShouldUseSaveCountAsRecipeRankingTieBreaker()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var owner = await CreateRankingUserAsync(context, "recipe-save-owner");
        var rater = await CreateRankingUserAsync(context, "recipe-save-rater");
        var firstUser = await CreateRankingUserAsync(context, "recipe-save-user-one");
        var secondUser = await CreateRankingUserAsync(context, "recipe-save-user-two");

        var firstRecipe = await CreateRecipeAsync(context, owner.Id, true);
        var secondRecipe = await CreateRecipeAsync(context, owner.Id, true);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        context.RecipeRatings.AddRange(
            new RecipeRating
            {
                RecipeId = firstRecipe.Id,
                UserId = rater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RecipeRating
            {
                RecipeId = secondRecipe.Id,
                UserId = rater.Id,
                Value = 5,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        context.UserRecipeFavorites.AddRange(
            new UserRecipeFavorite
            {
                RecipeId = firstRecipe.Id,
                UserId = firstUser.Id,
                CreatedAt = now
            },
            new UserRecipeFavorite
            {
                RecipeId = secondRecipe.Id,
                UserId = firstUser.Id,
                CreatedAt = now
            },
            new UserRecipeFavorite
            {
                RecipeId = secondRecipe.Id,
                UserId = secondUser.Id,
                CreatedAt = now
            }
        );

        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/Ranking/recipes");

        response.EnsureSuccessStatusCode();

        var root = await ParseResponseRootAsync(response);
        var ranking = root.EnumerateArray().ToList();

        var firstRecipePosition = ranking
            .Single(item => item.GetProperty("recipeId").GetInt32() == firstRecipe.Id)
            .GetProperty("position")
            .GetInt32();

        var secondRecipePosition = ranking
            .Single(item => item.GetProperty("recipeId").GetInt32() == secondRecipe.Id)
            .GetProperty("position")
            .GetInt32();

        Assert.True(secondRecipePosition < firstRecipePosition);
    }
    
}