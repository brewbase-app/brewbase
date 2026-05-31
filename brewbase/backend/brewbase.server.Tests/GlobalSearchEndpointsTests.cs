using System.Net;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class GlobalSearchEndpointsTests : IDisposable
{
    private const int User1 = 1;
    private const int User2 = 2;
    private const int AdminUser = 3;

    private readonly RecipeApiFactory _factory;
    private readonly HttpClient _client;

    public GlobalSearchEndpointsTests()
    {
        _factory = new RecipeApiFactory();
        _client = _factory.CreateClient();
        SeedSearchExtras();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Unauthenticated_Search_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/search?query=alpha");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EmptyQuery_ReturnsEmptyResults()
    {
        var response = await SearchAsUserAsync(User1, "   ");
        response.EnsureSuccessStatusCode();

        var root = await ParseJsonAsync(response);
        Assert.Equal(string.Empty, root.GetProperty("query").GetString());
        Assert.Empty(root.GetProperty("results").EnumerateArray());
    }

    [Fact]
    public async Task QueryShorterThanTwoCharacters_ReturnsEmptyResults()
    {
        var response = await SearchAsUserAsync(User1, "a");
        response.EnsureSuccessStatusCode();

        var root = await ParseJsonAsync(response);
        Assert.Empty(root.GetProperty("results").EnumerateArray());
    }

    [Fact]
    public async Task User1_Search_FindsCoffeeByName()
    {
        var results = await SearchResultsAsync(User1, "Alpha");
        Assert.Contains(results, item =>
            item.GetProperty("type").GetString() == "coffee"
            && item.GetProperty("title").GetString() == "Alpha Coffee");
    }

    [Fact]
    public async Task User1_Search_FindsRecipeByTitle()
    {
        var results = await SearchResultsAsync(User1, "Alpha Recipe");
        Assert.Contains(results, item =>
            item.GetProperty("type").GetString() == "recipe"
            && item.GetProperty("title").GetString() == "Alpha Recipe");
    }

    [Fact]
    public async Task User1_Search_FindsWikiArticleByTitle()
    {
        var results = await SearchResultsAsync(User1, "Krajowa");
        Assert.Contains(results, item =>
            item.GetProperty("type").GetString() == "wiki"
            && item.GetProperty("title").GetString() == "Krajowa wiki kawy");
    }

    [Fact]
    public async Task User1_Search_FindsOwnQuickNote_NotOtherUsers()
    {
        var results = await SearchResultsAsync(User1, "jasmin");
        var quickNotes = results
            .Where(item => item.GetProperty("type").GetString() == "quick_note")
            .ToList();

        Assert.Single(quickNotes);
        Assert.Equal(11, quickNotes[0].GetProperty("id").GetInt32());

        var user1Secret = await SearchResultsAsync(User1, "secret");
        Assert.DoesNotContain(user1Secret, item =>
            item.GetProperty("type").GetString() == "quick_note");

        var user2Secret = await SearchResultsAsync(User2, "secret");
        Assert.Contains(user2Secret, item =>
            item.GetProperty("type").GetString() == "quick_note"
            && item.GetProperty("id").GetInt32() == 12);
    }

    [Fact]
    public async Task User1_Search_DoesNotReturnOtherUsersPrivateRecipe()
    {
        var results = await SearchResultsAsync(User1, "Zulu");
        Assert.DoesNotContain(results, item =>
            item.GetProperty("type").GetString() == "recipe"
            && item.GetProperty("title").GetString() == "Zulu Recipe");
    }

    [Fact]
    public async Task Admin_Search_DoesNotReturnOtherUsersPrivateRecipe()
    {
        var results = await SearchResultsAsync(AdminUser, "Zulu");
        Assert.DoesNotContain(results, item =>
            item.GetProperty("type").GetString() == "recipe"
            && item.GetProperty("title").GetString() == "Zulu Recipe");
    }

    [Fact]
    public async Task User1_Search_MatchesWithoutPolishDiacritics()
    {
        var results = await SearchResultsAsync(User1, "czesc");
        Assert.Contains(results, item =>
            item.GetProperty("type").GetString() == "recipe"
            && item.GetProperty("title").GetString() == "Receptura z częścią");
    }

    [Fact]
    public async Task User1_Search_IsCaseInsensitive()
    {
        var results = await SearchResultsAsync(User1, "ETIOPIA");
        Assert.Contains(results, item =>
            item.GetProperty("type").GetString() == "quick_note");
    }

    private void SeedSearchExtras()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();

        if (!context.Articles.Any(article => article.Title == "Krajowa wiki kawy"))
        {
            context.Articles.Add(new Article
            {
                Title = "Krajowa wiki kawy",
                Content = "Opis o niskiej kwasowości i profilu smakowym.",
                Status = "Approved",
                Module = "country",
                UserId = User1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });
        }

        if (!context.Recipes.Any(recipe => recipe.Title == "Receptura z częścią"))
        {
            context.Recipes.Add(new Recipe
            {
                Title = "Receptura z częścią",
                Parameters = "{\"coffee_grams\":18}",
                Steps = "Krok z opisem części wody.",
                IsPublic = true,
                UserId = User1,
                CoffeeId = 1,
                BrewingMethodId = 1,
                CreatedAt = DateTime.UtcNow
            });
        }

        var hasEtiopiaNote = context.QuickNotes
            .AsEnumerable()
            .Any(note =>
                note.UserId == User1
                && note.Content.Contains("Etiopia", StringComparison.OrdinalIgnoreCase));

        if (!hasEtiopiaNote)
        {
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            context.QuickNotes.Add(new QuickNote
            {
                Content = "Notatka o Etiopii i jaśminie.",
                UserId = User1,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        context.SaveChanges();
    }

    private async Task<HttpResponseMessage> SearchAsUserAsync(int userId, string query)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/search?query={Uri.EscapeDataString(query)}");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, userId.ToString());
        return await _client.SendAsync(request);
    }

    private async Task<List<JsonElement>> SearchResultsAsync(int userId, string query)
    {
        var response = await SearchAsUserAsync(userId, query);
        response.EnsureSuccessStatusCode();
        var root = await ParseJsonAsync(response);
        return root.GetProperty("results").EnumerateArray().ToList();
    }

    private static async Task<JsonElement> ParseJsonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<JsonElement>(stream);
    }
}
