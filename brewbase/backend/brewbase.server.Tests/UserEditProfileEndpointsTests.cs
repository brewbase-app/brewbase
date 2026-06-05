using System.Net;
using System.Net.Http.Json;
using brewbase.server.Models;
using brewbase.server.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace brewbase.server.Tests;

public class UserEditProfileEndpointsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;
    private readonly HttpClient _client;

    public UserEditProfileEndpointsTests()
    {
        _factory = new CoffeeApiFactory();
        _client = _factory.CreateAuthenticatedClient(userId: 1, role: "User");
        SeedSecondUser();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task EditProfile_DuplicateLogin_ReturnsConflict()
    {
        var response = await _client.PutAsJsonAsync("/api/users/edit_profile", new
        {
            login = "admin.tester",
            email = "coffee.tester@brewbase.local"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var user = await context.AppUsers.SingleAsync(u => u.Id == 1);

        Assert.Equal("coffee.tester", user.Login);
        Assert.Equal("coffee.tester@brewbase.local", user.Email);
    }

    [Fact]
    public async Task EditProfile_DuplicateEmail_ReturnsConflict()
    {
        var response = await _client.PutAsJsonAsync("/api/users/edit_profile", new
        {
            login = "coffee.tester",
            email = "admin.tester@brewbase.local"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var user = await context.AppUsers.SingleAsync(u => u.Id == 1);

        Assert.Equal("coffee.tester", user.Login);
        Assert.Equal("coffee.tester@brewbase.local", user.Email);
    }

    [Fact]
    public async Task EditProfile_SameLoginAsCurrentUser_IsAllowed()
    {
        var response = await _client.PutAsJsonAsync("/api/users/edit_profile", new
        {
            login = "coffee.tester",
            email = "coffee.tester@brewbase.local"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var user = await context.AppUsers.SingleAsync(u => u.Id == 1);

        Assert.Equal("coffee.tester", user.Login);
        Assert.Equal("coffee.tester@brewbase.local", user.Email);
    }

    [Fact]
    public async Task EditProfile_SameEmailAsCurrentUser_IsAllowed()
    {
        const string email = "coffee.tester@brewbase.local";

        var response = await _client.PutAsJsonAsync("/api/users/edit_profile", new
        {
            login = "coffee.tester",
            email
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
        var user = await context.AppUsers.SingleAsync(u => u.Id == 1);

        Assert.Equal(email, user.Email);
    }

    private void SeedSecondUser()
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
}
