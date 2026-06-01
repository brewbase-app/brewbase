using System.Linq;
using System.Net;
using System.Text.Json;
using brewbase.server.Tests.Infrastructure;
using Xunit;
using System.Net.Http.Json;
using brewbase.server.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Tests;

public class CoffeeEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public CoffeeEndpointsTests()
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
    public async Task ShouldReturnListOfCoffees()
    {
        var response = await _client.GetAsync("/api/Coffee");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(JsonValueKind.Array, coffees.ValueKind);
        Assert.True(coffees.GetArrayLength() > 0);

        var first = coffees[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("isVerified", out _));
        Assert.True(first.TryGetProperty("region", out _));
        Assert.True(first.TryGetProperty("roastery", out _));
        Assert.True(first.TryGetProperty("processingMethod", out _));
        Assert.True(first.TryGetProperty("variety", out _));
        Assert.True(first.TryGetProperty("createdByUserId", out _));
    }

    [Fact]
    public async Task ShouldReturnCoffeeDetailsForValidId()
    {
        var validId = 1;
        var response = await _client.GetAsync($"/api/Coffee/{validId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffee = document.RootElement;

        Assert.Equal(validId, coffee.GetProperty("id").GetInt32());
        Assert.True(coffee.TryGetProperty("name", out _));
        Assert.True(coffee.TryGetProperty("isVerified", out _));
        Assert.True(coffee.TryGetProperty("region", out _));
        Assert.True(coffee.TryGetProperty("roastery", out _));
        Assert.True(coffee.TryGetProperty("processingMethod", out _));
        Assert.True(coffee.TryGetProperty("variety", out _));
        Assert.True(coffee.TryGetProperty("createdByUserId", out _));
    }

	[Fact]
	public async Task ShouldReturnNullAverageRatingAndZeroRatingCountForCoffeeWithoutRatings()
	{
    	using var scope = _factory.Services.CreateScope();
    	var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    	var coffee = new Coffee
    	{
        	Name = $"No Rating Coffee {Guid.NewGuid()}",
        	IsVerified = true,
        	RegionId = 1,
        	RoasteryId = 1,
        	CreatedByUserId = 1
    	};

    	context.Coffees.Add(coffee);
    	await context.SaveChangesAsync();

    	var response = await _client.GetAsync($"/api/Coffee/{coffee.Id}");

    	response.EnsureSuccessStatusCode();

    	var payload = await response.Content.ReadAsStringAsync();
    	using var document = JsonDocument.Parse(payload);
    	var root = document.RootElement;

    	Assert.Equal(JsonValueKind.Null, root.GetProperty("averageRating").ValueKind);
    	Assert.Equal(0, root.GetProperty("ratingCount").GetInt32());
	}

	[Fact]
	public async Task ShouldReturnAverageRatingAndRatingCountForCoffeeWithRatings()
	{
    	using var scope = _factory.Services.CreateScope();
    	var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

    	if (!await context.AppUsers.AnyAsync(user => user.Id == 2))
    	{
        	context.AppUsers.Add(new AppUser
        	{
            	Id = 2,
            	Login = "coffee.rating.user.two",
            	Email = "coffee.rating.user.two@brewbase.local",
            	PasswordHash = "test-hash",
            	Role = "User",
            	CreatedAt = DateTime.UtcNow
        	});
    	}

    	var coffee = new Coffee
    	{
        	Name = $"Rated Coffee {Guid.NewGuid()}",
        	IsVerified = true,
        	RegionId = 1,
        	RoasteryId = 1,
        	CreatedByUserId = 1
    	};

    	context.Coffees.Add(coffee);
    	await context.SaveChangesAsync();

    	var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    	context.CoffeeRatings.AddRange(
        	new CoffeeRating
        	{
            	CoffeeId = coffee.Id,
            	UserId = 1,
            	Value = 4,
            	CreatedAt = now,
            	UpdatedAt = now
        	},
        	new CoffeeRating
        	{
            	CoffeeId = coffee.Id,
            	UserId = 2,
            	Value = 5,
            	CreatedAt = now,
            	UpdatedAt = now
        	}
    	);

    	await context.SaveChangesAsync();

    	var response = await _client.GetAsync($"/api/Coffee/{coffee.Id}");

    	response.EnsureSuccessStatusCode();

    	var payload = await response.Content.ReadAsStringAsync();
    	using var document = JsonDocument.Parse(payload);
    	var root = document.RootElement;

    	Assert.Equal(4.5, root.GetProperty("averageRating").GetDouble());
    	Assert.Equal(2, root.GetProperty("ratingCount").GetInt32());
	}

    [Fact]
    public async Task ShouldReturnNotFoundForNonExistingId()
    {
        var response = await _client.GetAsync("/api/Coffee/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldFilterCoffeesByRegionId()
    {
        var response = await _client.GetAsync("/api/Coffee?regionId=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(2, coffees.GetArrayLength());
        Assert.All(coffees.EnumerateArray(), coffee =>
        {
            Assert.Equal("North Region", coffee.GetProperty("region").GetString());
        });
    }

    [Fact]
    public async Task ShouldFilterCoffeesByRoasteryId()
    {
        var response = await _client.GetAsync("/api/Coffee?roasteryId=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(2, coffees.GetArrayLength());
        Assert.All(coffees.EnumerateArray(), coffee =>
        {
            Assert.Equal("Roastery One", coffee.GetProperty("roastery").GetString());
        });
    }

    [Fact(Skip = "Temporary disabled: EF.Functions.ILike is not translated by SQLite in integration tests.")]
    public async Task ShouldSearchCoffeesByName()
    {
        var response = await _client.GetAsync("/api/Coffee?search=beta");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Single(coffees.EnumerateArray());
        Assert.Equal("Beta Coffee", coffees[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ShouldSortCoffeesByNameAscending()
    {
        var response = await _client.GetAsync("/api/Coffee?sortBy=name&sortOrder=asc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        var names = coffees.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Alpha Coffee", "Beta Coffee", "Zulu Coffee" }, names);
    }

    [Fact]
    public async Task ShouldSortCoffeesByNameDescending()
    {
        var response = await _client.GetAsync("/api/Coffee?sortBy=name&sortOrder=desc");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        var names = coffees.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Equal(new List<string?> { "Zulu Coffee", "Beta Coffee", "Alpha Coffee" }, names);
    }

    [Fact]
    public async Task ShouldPaginateCoffees()
    {
        var response = await _client.GetAsync("/api/Coffee?page=2&pageSize=1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Single(coffees.EnumerateArray());
        Assert.Equal("Beta Coffee", coffees[0].GetProperty("name").GetString());
    }
    
    [Fact]
    public async Task ShouldRateOwnCoffee_ReturnsForbidden()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/Coffee/1/rating", new { value = 4 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShouldRateOthersCoffee_ReturnsNoContent()
    {
        var client = _factory.CreateAuthenticatedClient(userId: 2);

        var response = await client.PostAsJsonAsync("/api/Coffee/1/rating", new { value = 4 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var rating = context.CoffeeRatings.Single(r => r.CoffeeId == 1 && r.UserId == 2);

        Assert.Equal(4, rating.Value);
    }

    [Fact]
    public async Task ShouldUpdateExistingCoffeeRating()
    {
        var client = _factory.CreateAuthenticatedClient(userId: 2);

        var firstResponse = await client.PostAsJsonAsync("/api/Coffee/2/rating", new { value = 2 });
        var secondResponse = await client.PostAsJsonAsync("/api/Coffee/2/rating", new { value = 5 });

        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var ratings = context.CoffeeRatings
            .Where(r => r.CoffeeId == 2 && r.UserId == 2)
            .ToList();

        Assert.Single(ratings);
        Assert.Equal(5, ratings[0].Value);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenRatingNonExistingCoffee()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/Coffee/999999/rating", new { value = 4 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenCoffeeRatingIsOutsideRange()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/Coffee/3/rating", new { value = 6 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_AddCoffeeFavorite_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/Coffee/1/favorite", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAddCoffeeFavorite()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsync("/api/Coffee/1/favorite", null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.True(await context.UserCoffeeFavorites.AnyAsync(f => f.UserId == 1 && f.CoffeeId == 1));
    }

    [Fact]
    public async Task DuplicateAddCoffeeFavorite_IsIdempotent()
    {
        var client = _factory.CreateAuthenticatedClient();

        var first = await client.PostAsync("/api/Coffee/1/favorite", null);
        var second = await client.PostAsync("/api/Coffee/1/favorite", null);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.Equal(1, await context.UserCoffeeFavorites.CountAsync(f => f.UserId == 1 && f.CoffeeId == 1));
    }

    [Fact]
    public async Task ShouldRemoveCoffeeFavorite()
    {
        var client = _factory.CreateAuthenticatedClient();

        await client.PostAsync("/api/Coffee/2/favorite", null);

        var response = await client.DeleteAsync("/api/Coffee/2/favorite");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.False(await context.UserCoffeeFavorites.AnyAsync(f => f.UserId == 1 && f.CoffeeId == 2));
    }

    [Fact]
    public async Task RemoveMissingCoffeeFavorite_IsIdempotent()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.DeleteAsync("/api/Coffee/2/favorite");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ShouldListFavoriteCoffees()
    {
        var client = _factory.CreateAuthenticatedClient();

        await client.PostAsync("/api/Coffee/1/favorite", null);
        await client.PostAsync("/api/Coffee/3/favorite", null);

        var response = await client.GetAsync("/api/Coffee/favorites");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var favorites = document.RootElement.EnumerateArray().ToList();

        Assert.Equal(2, favorites.Count);
        Assert.All(favorites, item => Assert.True(item.GetProperty("isFavorite").GetBoolean()));
    }

    [Fact]
    public async Task AddFavoriteForNonExistingCoffee_ReturnsNotFound()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsync("/api/Coffee/999999/favorite", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnIsFavoriteOnCoffeeDetailWhenAuthenticated()
    {
        var client = _factory.CreateAuthenticatedClient();

        await client.PostAsync("/api/Coffee/1/favorite", null);

        var response = await client.GetAsync("/api/Coffee/1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);

        Assert.True(document.RootElement.GetProperty("isFavorite").GetBoolean());
    }

    [Fact]
    public async Task CoffeeFavorite_UnfavoriteAndRefavorite_UpdatesListAndDetail()
    {
        var client = _factory.CreateAuthenticatedClient();

        await client.PostAsync("/api/Coffee/1/favorite", null);

        var favorited = await client.GetAsync("/api/Coffee/favorites");
        favorited.EnsureSuccessStatusCode();
        var favoritedPayload = await favorited.Content.ReadAsStringAsync();
        using (var favoritedDoc = JsonDocument.Parse(favoritedPayload))
        {
            Assert.Contains(
                favoritedDoc.RootElement.EnumerateArray(),
                c => c.GetProperty("id").GetInt32() == 1);
        }

        var unfavorite = await client.DeleteAsync("/api/Coffee/1/favorite");
        Assert.Equal(HttpStatusCode.NoContent, unfavorite.StatusCode);

        var afterRemove = await client.GetAsync("/api/Coffee/favorites");
        afterRemove.EnsureSuccessStatusCode();
        var afterRemovePayload = await afterRemove.Content.ReadAsStringAsync();
        using (var afterRemoveDoc = JsonDocument.Parse(afterRemovePayload))
        {
            Assert.DoesNotContain(
                afterRemoveDoc.RootElement.EnumerateArray(),
                c => c.GetProperty("id").GetInt32() == 1);
        }

        var detailAfterRemove = await client.GetAsync("/api/Coffee/1");
        detailAfterRemove.EnsureSuccessStatusCode();
        var detailPayload = await detailAfterRemove.Content.ReadAsStringAsync();
        using (var detailDoc = JsonDocument.Parse(detailPayload))
        {
            Assert.False(detailDoc.RootElement.GetProperty("isFavorite").GetBoolean());
        }

        var refavorite = await client.PostAsync("/api/Coffee/1/favorite", null);
        Assert.Equal(HttpStatusCode.NoContent, refavorite.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        Assert.True(await context.UserCoffeeFavorites.AnyAsync(f => f.UserId == 1 && f.CoffeeId == 1));
    }

    [Fact]
    public async Task CoffeeFavoriteRowCount_MatchesRankingInputAfterAddAndRemove()
    {
        var client = _factory.CreateAuthenticatedClient(userId: 1);

        await client.PostAsync("/api/Coffee/1/favorite", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            Assert.Equal(1, await context.UserCoffeeFavorites.CountAsync(f => f.CoffeeId == 1));
        }

        await client.DeleteAsync("/api/Coffee/1/favorite");

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            Assert.Equal(0, await context.UserCoffeeFavorites.CountAsync(f => f.CoffeeId == 1));
        }
    }
}