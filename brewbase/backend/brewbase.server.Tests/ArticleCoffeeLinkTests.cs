using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class ArticleCoffeeLinkTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public ArticleCoffeeLinkTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
        _authenticatedClient = _factory.CreateAuthenticatedClient();
    }

    public void Dispose()
    {
        _authenticatedClient.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldCreateCoffeeArticleWithValidCoffeeId()
    {
        var request = new CreateArticleRequestDto
        {
            Title = "Linked Alpha Article",
            Content = "Wiki body for Alpha Coffee.",
            Module = "coffee",
            CoffeeId = 1
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/articles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var article = await context.Articles
            .OrderByDescending(a => a.Id)
            .FirstAsync();

        Assert.Equal("Pending", article.Status);
        Assert.Equal(1, article.CoffeeId);
    }

    [Fact]
    public async Task ShouldCreateCoffeeArticleWithoutCoffeeId()
    {
        var request = new CreateArticleRequestDto
        {
            Title = "Standalone Coffee Article",
            Content = "Wiki body without catalog link.",
            Module = "coffee"
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/articles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var article = await context.Articles
            .OrderByDescending(a => a.Id)
            .FirstAsync();

        Assert.Null(article.CoffeeId);
    }

    [Fact]
    public async Task ShouldRejectInvalidCoffeeIdForCoffeeArticle()
    {
        var request = new CreateArticleRequestDto
        {
            Title = "Broken Link Article",
            Content = "Wiki body.",
            Module = "coffee",
            CoffeeId = 9999
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/articles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("coffeeId", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShouldRejectCoffeeIdForNonCoffeeModule()
    {
        var request = new CreateArticleRequestDto
        {
            Title = "Country Article",
            Content = "Country wiki body.",
            Module = "country",
            CoffeeId = 1
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/articles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("coffeeId", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShouldReturnCoffeeLookupMatchesByName()
    {
        var response = await _client.GetAsync("/api/Coffee/lookup?name=Alpha");

        response.EnsureSuccessStatusCode();

        var matches = await response.Content.ReadFromJsonAsync<List<CoffeeLookupResponseDto>>();

        Assert.NotNull(matches);
        Assert.Contains(matches, match => match.Id == 1 && match.Name == "Alpha Coffee");
    }

    [Fact]
    public async Task ShouldReturnLinkedApprovedArticleOnCoffeeDetail()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "Alpha Wiki",
                Content = "Approved wiki content for Alpha Coffee.",
                Module = "coffee",
                Status = "Approved",
                CoffeeId = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/Coffee/1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffee = document.RootElement;

        Assert.True(coffee.TryGetProperty("wikiArticle", out var wikiArticle));
        Assert.Equal(
            "Approved wiki content for Alpha Coffee.",
            wikiArticle.GetProperty("content").GetString());
        Assert.Equal("coffee.tester", wikiArticle.GetProperty("authorLogin").GetString());
    }

    [Fact]
    public async Task ShouldNotReturnPendingLinkedArticleOnCoffeeDetail()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "Pending Alpha Wiki",
                Content = "Pending wiki content.",
                Module = "coffee",
                Status = "Pending",
                CoffeeId = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/Coffee/1");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffee = document.RootElement;

        if (coffee.TryGetProperty("wikiArticle", out var wikiArticle))
        {
            Assert.Equal(JsonValueKind.Null, wikiArticle.ValueKind);
        }
    }

    [Fact]
    public async Task ShouldExcludeLinkedApprovedCoffeeArticlesFromPublicList()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "Public Linked Coffee Article",
                Content = "Public linked content.",
                Module = "coffee",
                Status = "Approved",
                CoffeeId = 2,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/articles?module=coffee");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var articles = document.RootElement;

        Assert.Equal(JsonValueKind.Array, articles.ValueKind);

        var linkedArticle = articles.EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("title", out var title)
                && title.GetString() == "Public Linked Coffee Article");

        Assert.Equal(JsonValueKind.Undefined, linkedArticle.ValueKind);
    }

    [Fact]
    public async Task ShouldIncludeUnlinkedApprovedCoffeeArticlesInPublicList()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "Public Unlinked Coffee Article",
                Content = "Standalone wiki content.",
                Module = "coffee",
                Status = "Approved",
                CoffeeId = null,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/articles?module=coffee");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var articles = document.RootElement;

        var unlinkedArticle = articles.EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("title", out var title)
                && title.GetString() == "Public Unlinked Coffee Article");

        Assert.True(unlinkedArticle.ValueKind != JsonValueKind.Undefined);
        Assert.False(unlinkedArticle.TryGetProperty("coffeeId", out var coffeeId)
            && coffeeId.ValueKind != JsonValueKind.Null);
    }

    [Fact]
    public async Task ShouldReturnCoffeeIdOnMyArticleListAndDetail()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "My Linked Coffee Draft",
                Content = "Pending linked content.",
                Module = "coffee",
                Status = "Pending",
                CoffeeId = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var listResponse = await _authenticatedClient.GetAsync("/api/articles/mine");
        listResponse.EnsureSuccessStatusCode();

        var listPayload = await listResponse.Content.ReadAsStringAsync();
        using var listDocument = JsonDocument.Parse(listPayload);
        var listItem = listDocument.RootElement.EnumerateArray()
            .First(item =>
                item.GetProperty("title").GetString() == "My Linked Coffee Draft");

        Assert.Equal(1, listItem.GetProperty("coffeeId").GetInt32());

        var articleId = listItem.GetProperty("id").GetInt32();
        var detailResponse = await _authenticatedClient.GetAsync($"/api/articles/mine/{articleId}");
        detailResponse.EnsureSuccessStatusCode();

        var detailPayload = await detailResponse.Content.ReadAsStringAsync();
        using var detailDocument = JsonDocument.Parse(detailPayload);

        Assert.Equal(1, detailDocument.RootElement.GetProperty("coffeeId").GetInt32());
    }

    [Fact]
    public async Task ShouldNotExcludeNonCoffeeModuleFromPublicList()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

            context.Articles.Add(new Article
            {
                Title = "Public Country Article",
                Content = "Region: Test\n\nCountry content.",
                Module = "country",
                Status = "Approved",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/articles?module=country");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);

        Assert.Contains(
            document.RootElement.EnumerateArray(),
            item => item.GetProperty("title").GetString() == "Public Country Article");
    }

    [Fact]
    public async Task ShouldStillAllowCreatingNonCoffeeArticleWithoutCoffeeId()
    {
        var request = new CreateArticleRequestDto
        {
            Title = "Poland Country Article",
            Content = "Region: Mazowsze\n\nOpis kraju.",
            Module = "country"
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/articles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
