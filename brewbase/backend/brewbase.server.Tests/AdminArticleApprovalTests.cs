using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class AdminArticleApprovalTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _adminClient;

    public AdminArticleApprovalTests()
    {
        _factory = new CoffeeApiFactory();
        _adminClient = _factory.CreateAuthenticatedClient(userId: 2, role: "Admin");
    }

    public void Dispose()
    {
        _adminClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ShouldApproveFirstLinkedCoffeeArticle()
    {
        var articleId = await SeedPendingCoffeeArticleAsync("First linked wiki", coffeeId: 1);

        var response = await _adminClient.PatchAsync(
            $"/api/admin/articles/{articleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var article = await context.Articles.SingleAsync(a => a.Id == articleId);

        Assert.Equal("Approved", article.Status);
        Assert.Equal(1, article.CoffeeId);
    }

    [Fact]
    public async Task ShouldRejectSecondLinkedCoffeeArticleApprovalForSameCoffee()
    {
        await SeedApprovedCoffeeArticleAsync("Existing approved wiki", coffeeId: 1);
        var pendingArticleId = await SeedPendingCoffeeArticleAsync(
            "Competing pending wiki",
            coffeeId: 1);

        var response = await _adminClient.PatchAsync(
            $"/api/admin/articles/{pendingArticleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(
            "approved wiki article",
            payload.GetProperty("message").GetString(),
            StringComparison.OrdinalIgnoreCase);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var pendingArticle = await context.Articles.SingleAsync(a => a.Id == pendingArticleId);

        Assert.Equal("Pending", pendingArticle.Status);
    }

    [Fact]
    public async Task ShouldAllowMultiplePendingLinkedArticlesForSameCoffee()
    {
        await SeedPendingCoffeeArticleAsync("Pending wiki A", coffeeId: 1);
        await SeedPendingCoffeeArticleAsync("Pending wiki B", coffeeId: 1);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var pendingCount = await context.Articles.CountAsync(article =>
            article.Module == "coffee"
            && article.Status == "Pending"
            && article.CoffeeId == 1);

        Assert.Equal(2, pendingCount);
    }

    [Fact]
    public async Task ShouldAllowMultipleRejectedLinkedArticlesForSameCoffee()
    {
        await SeedRejectedCoffeeArticleAsync("Rejected wiki A", coffeeId: 1);
        await SeedRejectedCoffeeArticleAsync("Rejected wiki B", coffeeId: 1);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var rejectedCount = await context.Articles.CountAsync(article =>
            article.Module == "coffee"
            && article.Status == "Rejected"
            && article.CoffeeId == 1);

        Assert.Equal(2, rejectedCount);
    }

    [Fact]
    public async Task ShouldAllowApprovingSameLinkedCoffeeArticleAgain()
    {
        var articleId = await SeedApprovedCoffeeArticleAsync(
            "Already approved wiki",
            coffeeId: 1);

        var response = await _adminClient.PatchAsync(
            $"/api/admin/articles/{articleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAllowApprovingUnlinkedCoffeeArticleWithoutCoffeeId()
    {
        var articleId = await SeedPendingCoffeeArticleAsync(
            "Unlinked coffee wiki",
            coffeeId: null);

        var response = await _adminClient.PatchAsync(
            $"/api/admin/articles/{articleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var article = await context.Articles.SingleAsync(a => a.Id == articleId);

        Assert.Equal("Approved", article.Status);
        Assert.NotNull(article.CoffeeId);
        Assert.True(await context.Coffees.AnyAsync(coffee => coffee.Id == article.CoffeeId));
    }

    [Fact]
    public async Task ShouldNotBlockNonCoffeeModuleApprovals()
    {
        var firstCountryArticleId = await SeedPendingArticleAsync(
            "Country wiki A",
            module: "country",
            coffeeId: null);
        var secondCountryArticleId = await SeedPendingArticleAsync(
            "Country wiki B",
            module: "country",
            coffeeId: null);

        var firstResponse = await _adminClient.PatchAsync(
            $"/api/admin/articles/{firstCountryArticleId}/approve",
            null);
        var secondResponse = await _adminClient.PatchAsync(
            $"/api/admin/articles/{secondCountryArticleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    private async Task<int> SeedPendingCoffeeArticleAsync(string title, int? coffeeId)
    {
        return await SeedPendingArticleAsync(title, "coffee", coffeeId);
    }

    private async Task<int> SeedPendingArticleAsync(
        string title,
        string module,
        int? coffeeId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        var article = new Article
        {
            Title = title,
            Content = $"{title} content",
            Module = module,
            Status = "Pending",
            CoffeeId = coffeeId,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article.Id;
    }

    private async Task<int> SeedApprovedCoffeeArticleAsync(string title, int coffeeId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        var article = new Article
        {
            Title = title,
            Content = $"{title} content",
            Module = "coffee",
            Status = "Approved",
            CoffeeId = coffeeId,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article.Id;
    }

    private async Task SeedRejectedCoffeeArticleAsync(string title, int coffeeId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        context.Articles.Add(new Article
        {
            Title = title,
            Content = $"{title} content",
            Module = "coffee",
            Status = "Rejected",
            CoffeeId = coffeeId,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
            ModerationComment = "Rejected for test."
        });

        await context.SaveChangesAsync();
    }
}
