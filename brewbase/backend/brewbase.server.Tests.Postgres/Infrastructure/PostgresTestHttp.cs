using System.Net.Http.Headers;
using brewbase.server.Services;

namespace brewbase.server.Tests.Postgres.Infrastructure;

internal static class PostgresTestHttp
{
    public const string RoleHeaderName = "X-Dev-User-Role";
    public const string LoginHeaderName = "X-Dev-User-Login";

    public static HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string url,
        int userId,
        string role = "User",
        string? login = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add(CurrentUserProvider.DevUserIdHeaderName, userId.ToString());
        request.Headers.Add(RoleHeaderName, role);

        if (!string.IsNullOrWhiteSpace(login))
        {
            request.Headers.Add(LoginHeaderName, login);
        }

        return request;
    }

    public static HttpRequestMessage CreateGet(
        string url,
        int userId,
        string role = "User",
        string? login = null) =>
        CreateAuthenticatedRequest(HttpMethod.Get, url, userId, role, login);

    public static HttpRequestMessage CreatePatch(
        string url,
        int userId,
        string role = "User",
        string? login = null) =>
        CreateAuthenticatedRequest(HttpMethod.Patch, url, userId, role, login);

    public static HttpRequestMessage CreatePost(
        string url,
        int userId,
        string role = "User",
        string? login = null) =>
        CreateAuthenticatedRequest(HttpMethod.Post, url, userId, role, login);

    public static void SetAuthenticatedUser(
        HttpClient client,
        int userId,
        string role = "User",
        string? login = null)
    {
        client.DefaultRequestHeaders.Remove(CurrentUserProvider.DevUserIdHeaderName);
        client.DefaultRequestHeaders.Remove(RoleHeaderName);
        client.DefaultRequestHeaders.Remove(LoginHeaderName);

        client.DefaultRequestHeaders.Add(CurrentUserProvider.DevUserIdHeaderName, userId.ToString());
        client.DefaultRequestHeaders.Add(RoleHeaderName, role);

        if (!string.IsNullOrWhiteSpace(login))
        {
            client.DefaultRequestHeaders.Add(LoginHeaderName, login);
        }
    }

    public static void ClearAuthenticatedUser(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove(CurrentUserProvider.DevUserIdHeaderName);
        client.DefaultRequestHeaders.Remove(RoleHeaderName);
        client.DefaultRequestHeaders.Remove(LoginHeaderName);
    }
}
