using System.Net;
using System.Net.Http.Json;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using brewbase.server.Models;
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

    private sealed class RegisterResponse
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Token { get; set; } = "";
    }
}
