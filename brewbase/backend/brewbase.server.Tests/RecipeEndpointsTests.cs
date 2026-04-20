using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using brewbase.server.Services;
using brewbase.server.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace brewbase.server.Tests;

public class RecipeEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly HttpClient _client;

    public RecipeEndpointsTests(CoffeeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldCreateRecipeAndReturnCreatedWithCurrentUserAsOwner()
    {
        var payload = new
        {
            title = "Integration test recipe",
            parameters = new { waterG = 250, ratio = "1:16" },
            steps = "1. Bloom\n2. Pour in spirals",
            isPublic = true,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/Recipe", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);
        Assert.Contains("/api/Recipe/", location!.ToString(), StringComparison.Ordinal);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.True(root.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Integration test recipe", root.GetProperty("title").GetString());
        Assert.Equal(1, root.GetProperty("userId").GetInt32());
        Assert.True(root.GetProperty("isPublic").GetBoolean());
        Assert.Equal("V60", root.GetProperty("brewingMethod").GetString());
        Assert.Equal("Test Coffee", root.GetProperty("coffee").GetString());
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedWhenCurrentUserIsNotResolved()
    {
        await using var factory = new CoffeeApiFactory().WithWebHostBuilder(builder =>
        {
            // Production: no dev user fallback; unauthenticated requests have no resolved user.
            builder.UseEnvironment("Production");
        });

        var client = factory.CreateClient();
        var payload = new
        {
            title = "No user recipe",
            parameters = new { },
            steps = "Step",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await client.PostAsJsonAsync("/api/Recipe", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdateRecipeSuccessfully()
    {
        var createPayload = new
        {
            title = "Before update",
            parameters = new { tempC = 92 },
            steps = "Old steps",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Recipe", createPayload);
        createResponse.EnsureSuccessStatusCode();
        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

        var updatePayload = new
        {
            title = "After update",
            parameters = new { waterG = 300 },
            steps = "1. New step",
            isPublic = true,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await _client.PutAsJsonAsync($"/api/Recipe/{recipeId}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.Equal(recipeId, root.GetProperty("id").GetInt32());
        Assert.Equal("After update", root.GetProperty("title").GetString());
        Assert.Equal(1, root.GetProperty("userId").GetInt32());
        Assert.True(root.GetProperty("isPublic").GetBoolean());
        Assert.Equal("V60", root.GetProperty("brewingMethod").GetString());
        Assert.Equal("Test Coffee", root.GetProperty("coffee").GetString());
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedWhenUserNotResolved()
    {
        await using var factory = new CoffeeApiFactory().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });

        var client = factory.CreateClient();
        var payload = new
        {
            title = "Updated",
            parameters = new { },
            steps = "Step",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await client.PutAsJsonAsync("/api/Recipe/1", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnForbiddenWhenUserIsNotOwner()
    {
        await using var factory = new CoffeeApiFactory().WithWebHostBuilder(builder =>
        {
            // Default fixture pins DevUser:UserId = 1, which is evaluated before X-Dev-User-Id.
            // Override to a non-positive value so the same app + in-memory DB can resolve user 1 vs 2 per request.
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?> { ["DevUser:UserId"] = "0" });
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CurrentUserProvider.DevUserIdHeaderName, "1");

        var createPayload = new
        {
            title = "Owned by user 1",
            parameters = new { },
            steps = "Steps",
            isPublic = true,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var createResponse = await client.PostAsJsonAsync("/api/Recipe", createPayload);
        createResponse.EnsureSuccessStatusCode();
        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

        client.DefaultRequestHeaders.Remove(CurrentUserProvider.DevUserIdHeaderName);
        client.DefaultRequestHeaders.Add(CurrentUserProvider.DevUserIdHeaderName, "2");

        var updatePayload = new
        {
            title = "Hijack attempt",
            parameters = new { },
            steps = "Nope",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await client.PutAsJsonAsync($"/api/Recipe/{recipeId}", updatePayload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenRecipeDoesNotExist()
    {
        var payload = new
        {
            title = "Ghost recipe",
            parameters = new { },
            steps = "Steps",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var response = await _client.PutAsJsonAsync("/api/Recipe/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldDeleteRecipeSuccessfully()
    {
        var createPayload = new
        {
            title = "To delete",
            parameters = new { },
            steps = "Steps",
            isPublic = false,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Recipe", createPayload);
        createResponse.EnsureSuccessStatusCode();
        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/Recipe/{recipeId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Recipe/{recipeId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedWhenDeletingWithoutUser()
    {
        await using var factory = new CoffeeApiFactory().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });

        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/Recipe/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnForbiddenWhenDeletingNotOwnedRecipe()
    {
        await using var factory = new CoffeeApiFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?> { ["DevUser:UserId"] = "0" });
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CurrentUserProvider.DevUserIdHeaderName, "1");

        var createPayload = new
        {
            title = "Owned by user 1 for delete",
            parameters = new { },
            steps = "Steps",
            isPublic = true,
            coffeeId = 1,
            brewingMethodId = 1
        };

        var createResponse = await client.PostAsJsonAsync("/api/Recipe", createPayload);
        createResponse.EnsureSuccessStatusCode();
        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

        client.DefaultRequestHeaders.Remove(CurrentUserProvider.DevUserIdHeaderName);
        client.DefaultRequestHeaders.Add(CurrentUserProvider.DevUserIdHeaderName, "2");

        var response = await client.DeleteAsync($"/api/Recipe/{recipeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenDeletingNonExistingRecipe()
    {
        var response = await _client.DeleteAsync("/api/Recipe/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
