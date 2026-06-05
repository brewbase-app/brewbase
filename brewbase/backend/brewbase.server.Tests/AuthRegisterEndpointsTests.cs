using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using brewbase.server.Models;
using brewbase.server.Services.Validation;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace brewbase.server.Tests;

public class AuthRegisterEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public AuthRegisterEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Register_ReturnsTokenForImmediateLogin()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        var response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login = $"register-{unique}",
            email = $"register-{unique}@brewbase.local",
            password = "secret123",
            passwordHint = "moja podpowiedz",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));
        Assert.Equal($"register-{unique}", payload.Login);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload.Token);

        var profileResponse = await _client.GetAsync("/api/users/profile_info");
        profileResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Register_DuplicateLogin_ReturnsConflict()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var login = $"duplicate-login-{unique}";

        var first = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login,
            email = $"first-{unique}@brewbase.local",
            password = "secret123",
            passwordHint = "hint",
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login,
            email = $"second-{unique}@brewbase.local",
            password = "secret123",
            passwordHint = "hint",
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var email = $"duplicate-email-{unique}@brewbase.local";

        var first = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login = $"first-{unique}",
            email,
            password = "secret123",
            passwordHint = "hint",
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login = $"second-{unique}",
            email,
            password = "secret123",
            passwordHint = "hint",
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Login_BlockedUser_ReturnsForbidden()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var login = $"blocked-{unique}";
        const string password = "secret123";

        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            login,
            email = $"{login}@brewbase.local",
            password,
            passwordHint = "hint",
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            var user = await context.AppUsers.SingleAsync(u => u.Login == login);
            user.IsBlocked = true;
            await context.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            login,
            password,
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    private sealed class RegisterResponse
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Token { get; set; } = "";
    }
}
