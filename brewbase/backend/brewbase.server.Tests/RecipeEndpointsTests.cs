using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using brewbase.server.Tests.Infrastructure;

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
}
