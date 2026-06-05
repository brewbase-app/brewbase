using System.Net;
using System.Net.Http.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class ArticleDeleteMineEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _ownerClient;
    private readonly HttpClient _otherUserClient;

    public ArticleDeleteMineEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _ownerClient = _factory.CreateAuthenticatedClient(userId: 1, role: "User");
        _otherUserClient = _factory.CreateAuthenticatedClient(userId: 3, role: "User");
        SeedOtherUser();
    }

    public void Dispose()
    {
        _otherUserClient.Dispose();
        _ownerClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task DeleteMine_PendingArticle_RemovesArticle()
    {
        var articleId = await SeedPendingArticleAsync(userId: 1, title: "Pending delete me");

        var response = await _ownerClient.DeleteAsync($"/api/articles/mine/{articleId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.False(await context.Articles.AnyAsync(article => article.Id == articleId));
    }

    [Fact]
    public async Task DeleteMine_PendingArticle_RemovesAssociatedReports()
    {
        var articleId = await SeedPendingArticleAsync(userId: 1, title: "Pending with report");

        var createReport = await _ownerClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody(articleId));
        Assert.Equal(HttpStatusCode.OK, createReport.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            Assert.Equal(1, await context.Reports.CountAsync(report => report.ArticleId == articleId));
        }

        var response = await _ownerClient.DeleteAsync($"/api/articles/mine/{articleId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.False(await verifyContext.Articles.AnyAsync(article => article.Id == articleId));
        Assert.Equal(0, await verifyContext.Reports.CountAsync(report => report.ArticleId == articleId));
    }

    [Fact]
    public async Task DeleteMine_ApprovedArticle_ReturnsForbidden()
    {
        var articleId = await SeedApprovedArticleAsync(userId: 1, title: "Approved delete blocked");

        var response = await _ownerClient.DeleteAsync($"/api/articles/mine/{articleId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.True(await context.Articles.AnyAsync(article => article.Id == articleId));
    }

    [Fact]
    public async Task DeleteMine_NotOwner_ReturnsNotFound()
    {
        var articleId = await SeedPendingArticleAsync(userId: 3, title: "Owned by other user");

        var response = await _ownerClient.DeleteAsync($"/api/articles/mine/{articleId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        Assert.True(await context.Articles.AnyAsync(article => article.Id == articleId));
    }

    private void SeedOtherUser()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (context.AppUsers.Any(user => user.Id == 3))
        {
            return;
        }

        context.AppUsers.Add(new AppUser
        {
            Id = 3,
            Login = "other.tester",
            Email = "other.tester@brewbase.local",
            PasswordHash = "test-hash",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
    }

    private async Task<int> SeedPendingArticleAsync(int userId, string title)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        var article = new Article
        {
            Title = title,
            Content = $"{title} content",
            Module = "country",
            Status = "Pending",
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article.Id;
    }

    private async Task<int> SeedApprovedArticleAsync(int userId, string title)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        var article = new Article
        {
            Title = title,
            Content = $"{title} content",
            Module = "country",
            Status = "Approved",
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article.Id;
    }

    private static CreateReportRequestDto ValidReportBody(int articleId) =>
        new()
        {
            ContentType = "article",
            ContentId = articleId,
            Category = "Spam lub reklama",
            Comment = "DeleteMine report cleanup test"
        };
}
