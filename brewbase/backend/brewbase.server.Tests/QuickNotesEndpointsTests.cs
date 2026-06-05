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
    public async Task User1_AfterDelete_GetById_ReturnsNotFound()
    {
        var body = """{"content":"Lifecycle note for delete then get"}""";
        using var post = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        post.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var postResponse = await _client.SendAsync(post);
        postResponse.EnsureSuccessStatusCode();
        var created = await ParseJsonAsync(postResponse);
        var id = created.GetProperty("id").GetInt32();

        using var delete = new HttpRequestMessage(HttpMethod.Delete, $"/api/QuickNotes/{id}");
        delete.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var deleteResponse = await _client.SendAsync(delete);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var get = new HttpRequestMessage(HttpMethod.Get, $"/api/QuickNotes/{id}");
        get.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var getResponse = await _client.SendAsync(get);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Create_ReturnsUnauthorized()
    {
        var body = """{"content":"Should fail"}""";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User1_Create_EmptyContent_ReturnsBadRequest()
    {
        var body = """{"content":"   "}""";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User1_GetById_Nonexistent_ReturnsNotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes/99999");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Create_ThenGetAll_PersistsNote()
    {
        var unique = $"Persist-{Guid.NewGuid():N}";
        var body = $$"""{"content":"{{unique}}"}""";
        using var post = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        post.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var postResponse = await _client.SendAsync(post);
        postResponse.EnsureSuccessStatusCode();

        using var list = new HttpRequestMessage(HttpMethod.Get, "/api/QuickNotes");
        list.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var listResponse = await _client.SendAsync(list);
        listResponse.EnsureSuccessStatusCode();

        var root = await ParseJsonAsync(listResponse);
        Assert.Contains(
            root.EnumerateArray(),
            item => item.GetProperty("content").GetString() == unique);
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

    [Fact]
    public async Task User1_Update_OwnNote_ReturnsOk()
    {
        var body = """{"content":"Updated content for note 11"}""";
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/QuickNotes/11")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = await ParseJsonAsync(response);
        Assert.Equal(11, root.GetProperty("id").GetInt32());
        Assert.Equal("Updated content for note 11", root.GetProperty("content").GetString());
    }

    [Fact]
    public async Task User1_Update_OtherUsersNote_ReturnsNotFound()
    {
        var body = """{"content":"Should not apply"}""";
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/QuickNotes/12")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User1_Update_InvalidContent_ReturnsBadRequest()
    {
        var body = """{"content":"   "}""";
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/QuickNotes/11")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User1_Delete_OwnNote_ReturnsNoContent()
    {
        var body = """{"content":"Temp for delete"}""";
        using var post = new HttpRequestMessage(HttpMethod.Post, "/api/QuickNotes")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        post.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var postResponse = await _client.SendAsync(post);
        postResponse.EnsureSuccessStatusCode();
        var created = await ParseJsonAsync(postResponse);
        var newId = created.GetProperty("id").GetInt32();

        using var delete = new HttpRequestMessage(HttpMethod.Delete, $"/api/QuickNotes/{newId}");
        delete.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var deleteResponse = await _client.SendAsync(delete);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task User1_Delete_OtherUsersNote_ReturnsNotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/QuickNotes/12");
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, User1.ToString());
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<JsonElement> ParseJsonAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}
