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

public class ReportEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _userClient;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _anonymousClient;

    public ReportEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _userClient = _factory.CreateAuthenticatedClient(userId: 1, role: "User");
        _adminClient = _factory.CreateAuthenticatedClient(userId: 2, role: "Admin");
        _anonymousClient = _factory.CreateClient();
    }

    public void Dispose()
    {
        _anonymousClient.Dispose();
        _userClient.Dispose();
        _adminClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Unauthenticated_CreateReport_ReturnsUnauthorized()
    {
        var response = await _anonymousClient.PostAsJsonAsync(
            "/api/reports",
            ValidReportBody(contentType: "article", contentId: 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_CreateArticleReport_ThenDuplicate_ReturnsConflict()
    {
        var articleId = await SeedApprovedArticleAsync("Reportable wiki");

        var first = await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var duplicate = await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task User_CreateReport_InvalidCategory_ReturnsBadRequest()
    {
        var articleId = await SeedApprovedArticleAsync("Invalid category wiki");

        var response = await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            new CreateReportRequestDto
            {
                Category = "Not a real category",
                Comment = "test"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User_CreateReport_MissingArticle_ReturnsNotFound()
    {
        var response = await _userClient.PostAsJsonAsync(
            "/api/reports/article/99999",
            ValidReportBody());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_GetOpenReports_IncludesNewReport()
    {
        var articleId = await SeedApprovedArticleAsync("Open queue wiki");

        await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());

        var response = await _adminClient.GetAsync("/api/admin/reports?scope=open");
        response.EnsureSuccessStatusCode();

        var reports = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(reports);
        Assert.Contains(reports, report =>
            report.GetProperty("contentType").GetString() == "article"
            && report.GetProperty("contentId").GetInt32() == articleId
            && report.GetProperty("status").GetString() == "open");
    }

    [Fact]
    public async Task Admin_DismissReport_MovesToHistory()
    {
        var articleId = await SeedApprovedArticleAsync("Dismiss flow wiki");
        await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());

        var reportId = await GetOpenReportIdForArticleAsync(articleId);

        var dismiss = await _adminClient.PatchAsync(
            $"/api/admin/reports/{reportId}/dismiss",
            null);
        Assert.Equal(HttpStatusCode.NoContent, dismiss.StatusCode);

        var history = await _adminClient.GetAsync("/api/admin/reports?scope=history");
        history.EnsureSuccessStatusCode();
        var resolved = await history.Content.ReadFromJsonAsync<List<JsonElement>>();

        Assert.Contains(resolved!, report =>
            report.GetProperty("reportId").GetInt32() == reportId
            && report.GetProperty("status").GetString() == "dismissed");
    }

    [Fact]
    public async Task Admin_DismissAlreadyResolvedReport_ReturnsConflict()
    {
        var articleId = await SeedApprovedArticleAsync("Double dismiss wiki");
        await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());
        var reportId = await GetOpenReportIdForArticleAsync(articleId);

        await _adminClient.PatchAsync($"/api/admin/reports/{reportId}/dismiss", null);

        var secondDismiss = await _adminClient.PatchAsync(
            $"/api/admin/reports/{reportId}/dismiss",
            null);

        Assert.Equal(HttpStatusCode.Conflict, secondDismiss.StatusCode);
    }

    [Fact]
    public async Task User_CannotAccessAdminReports_ReturnsForbidden()
    {
        var response = await _userClient.GetAsync("/api/admin/reports");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UpholdRecipeReport_MovesRecipeToDraftWithComment()
    {
        await SeedApprovedArticleAsync("Fallback wiki for recipe reports");
        var recipeId = await SeedPublicRecipeAsync("Reported recipe");
        var createReport = await _userClient.PostAsJsonAsync(
            "/api/reports",
            ValidReportBody(contentType: "recipe", contentId: recipeId));
        Assert.Equal(HttpStatusCode.OK, createReport.StatusCode);

        var reportId = await GetOpenReportIdForContentAsync("recipe", recipeId);

        var uphold = await _adminClient.PatchAsJsonAsync(
            $"/api/admin/reports/{reportId}/uphold",
            new ModerateArticleRequestDto
            {
                Comment = "Treść narusza zasady społeczności."
            });
        Assert.Equal(HttpStatusCode.NoContent, uphold.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        var recipe = await context.Recipes.SingleAsync(r => r.Id == recipeId);
        Assert.False(recipe.IsPublic);
        Assert.Equal("Treść narusza zasady społeczności.", recipe.ModerationComment);

        var authorRecipe = await _userClient.GetAsync($"/api/Recipe/{recipeId}");
        authorRecipe.EnsureSuccessStatusCode();
        var payload = await authorRecipe.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(payload.GetProperty("isPublic").GetBoolean());
        Assert.Equal(
            "Treść narusza zasady społeczności.",
            payload.GetProperty("moderationComment").GetString());
    }

    [Fact]
    public async Task Admin_UpholdReport_WithoutComment_ReturnsBadRequest()
    {
        var articleId = await SeedApprovedArticleAsync("Uphold comment required");
        await _userClient.PostAsJsonAsync(
            $"/api/reports/article/{articleId}",
            ValidReportBody());
        var reportId = await GetOpenReportIdForArticleAsync(articleId);

        var uphold = await _adminClient.PatchAsJsonAsync(
            $"/api/admin/reports/{reportId}/uphold",
            new ModerateArticleRequestDto
            {
                Comment = "   "
            });

        Assert.Equal(HttpStatusCode.BadRequest, uphold.StatusCode);
    }

    private async Task<int> SeedPublicRecipeAsync(string title)
    {
        var body = $$"""
            {"title":"{{title}}","parameters":{"coffee":"18g","water":"300ml","temperature":"94°C","brewTime":"3:30"},"steps":"1. Bloom\n2. Pour","isPublic":true,"coffeeId":1,"brewingMethodId":1}
            """;

        var response = await _userClient.PostAsync(
            "/api/Recipe",
            new StringContent(body, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var recipe = await response.Content.ReadFromJsonAsync<JsonElement>();
        return recipe!.GetProperty("id").GetInt32();
    }

    private async Task<int> GetOpenReportIdForContentAsync(string contentType, int contentId)
    {
        var response = await _adminClient.GetAsync("/api/admin/reports?scope=open");
        response.EnsureSuccessStatusCode();
        var reports = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        var match = reports!.First(report =>
            report.GetProperty("contentType").GetString() == contentType
            && report.GetProperty("contentId").GetInt32() == contentId);

        return match.GetProperty("reportId").GetInt32();
    }

    private static CreateReportRequestDto ValidReportBody(
        string contentType = "article",
        int contentId = 1) =>
        new()
        {
            ContentType = contentType,
            ContentId = contentId,
            Category = "Spam lub reklama",
            Comment = "Test report comment"
        };

    private async Task<int> SeedApprovedArticleAsync(string title)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var now = DateTime.UtcNow;

        var article = new Article
        {
            Title = title,
            Content = $"{title} body",
            Module = "country",
            Status = "Approved",
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };

        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article.Id;
    }

    private async Task<int> GetOpenReportIdForArticleAsync(int articleId)
    {
        var response = await _adminClient.GetAsync("/api/admin/reports?scope=open");
        response.EnsureSuccessStatusCode();
        var reports = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        var match = reports!.First(report =>
            report.GetProperty("contentType").GetString() == "article"
            && report.GetProperty("contentId").GetInt32() == articleId);

        return match.GetProperty("reportId").GetInt32();
    }
}
