using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using brewbase.server.Services;
using Xunit;

namespace brewbase.server.Tests;

public class QuickNotesEndpointsTests : IDisposable
{
    private const int User1 = 1;
    private const int User2 = 2;

    private readonly RecipeApiFactory _factory;
    private readonly HttpClient _client;

    public QuickNotesEndpointsTests()
    {
        _factory = new RecipeApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Unauthenticated_GetAll_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/QuickNotes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User1_GetAll_ReturnsOwnNotes_NewestFirst()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var root = await ParseJsonAsync(response);
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var list = root.EnumerateArray().ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(11, list[0].GetProperty("id").GetInt32());
        Assert.Equal(10, list[1].GetProperty("id").GetInt32());
    }

    [Fact(Skip = "SQLite does not support EF.Functions.ILike; verified on PostgreSQL")]
    public async Task User1_GetAll_WithSearch_FiltersContent()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes?search=ja%C5%9Bmin");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var root = await ParseJsonAsync(response);
        var list = root.EnumerateArray().ToList();
        Assert.Single(list);
        Assert.Equal(11, list[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task User1_GetById_OwnNote_ReturnsOk()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes/11");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var root = await ParseJsonAsync(response);
        Assert.Equal(11, root.GetProperty("id").GetInt32());
        Assert.Contains("jaśmin", root.GetProperty("content").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task User1_GetById_OtherUsersNote_ReturnsNotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes/12");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Create_ReturnsCreated()
    {
        var body = """{"content":"Nowa notatka z testu"}""";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var root = await ParseJsonAsync(response);
        Assert.True(root.GetProperty("id").GetInt32() > 0);
        Assert.Equal("Nowa notatka z testu", root.GetProperty("content").GetString());
    }

    private static async Task<JsonElement> ParseJsonAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}
