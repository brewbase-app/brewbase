using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class AdminModerationEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _userClient;

    public AdminModerationEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _adminClient = _factory.CreateAuthenticatedClient(userId: 2, role: "Admin");
        _userClient = _factory.CreateAuthenticatedClient(userId: 1, role: "User");
    }

    public void Dispose()
    {
        _userClient.Dispose();
        _adminClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task User_ApproveArticle_ReturnsForbidden()
    {
        var articleId = await SeedPendingArticleAsync("Forbidden approve");

        var response = await _userClient.PatchAsync(
            $"/api/admin/articles/{articleId}/approve",
            null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_RejectArticle_WithoutComment_ReturnsBadRequest()
    {
        var articleId = await SeedPendingArticleAsync("Reject without comment");

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/admin/articles/{articleId}/reject")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var response = await _adminClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_RejectArticle_WithComment_SetsDraftStatus()
    {
        var articleId = await SeedPendingArticleAsync("Reject with comment");

        var response = await _adminClient.PatchAsJsonAsync(
            $"/api/admin/articles/{articleId}/reject",
            new ModerateArticleRequestDto { Comment = "Needs more sources." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var article = await context.Articles.SingleAsync(a => a.Id == articleId);

        Assert.Equal("Draft", article.Status);
        Assert.Equal("Needs more sources.", article.ModerationComment);
    }

    [Fact]
    public async Task Admin_GetPendingArticles_IncludesSeededPending()
    {
        var title = $"Pending list {Guid.NewGuid():N}";
        await SeedPendingArticleAsync(title);

        var response = await _adminClient.GetAsync("/api/admin/articles/pending");
        response.EnsureSuccessStatusCode();

        var articles = await response.Content.ReadFromJsonAsync<List<PendingArticleResponseDto>>();
        Assert.Contains(articles!, article => article.Title == title);
    }

    [Fact]
    public async Task Admin_ApproveNonexistentArticle_ReturnsNotFound()
    {
        var response = await _adminClient.PatchAsync(
            "/api/admin/articles/99999/approve",
            null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<int> SeedPendingArticleAsync(string title)
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
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article.Id;
    }
}
