using System.Net;
using Xunit;
using System.Text.Json;
using brewbase.server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace brewbase.server.Tests;

public class CoffeeEndpointsTests : IClassFixture<CoffeeApiFactory>
{
    private readonly HttpClient _client;

    public CoffeeEndpointsTests(CoffeeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnListOfCoffees()
    {
        var response = await _client.GetAsync("/api/Coffee");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffees = document.RootElement;

        Assert.Equal(JsonValueKind.Array, coffees.ValueKind);
        Assert.True(coffees.GetArrayLength() > 0);

        var first = coffees[0];
        Assert.True(first.GetProperty("id").GetInt32() > 0);
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("isVerified", out _));
        Assert.True(first.TryGetProperty("region", out _));
        Assert.True(first.TryGetProperty("roastery", out _));
        Assert.True(first.TryGetProperty("processingMethod", out _));
        Assert.True(first.TryGetProperty("variety", out _));
        Assert.True(first.TryGetProperty("createdByUserId", out _));
    }

    [Fact]
    public async Task ShouldReturnCoffeeDetailsForValidId()
    {
        var validId = 1;
        var response = await _client.GetAsync($"/api/Coffee/{validId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var coffee = document.RootElement;

        Assert.Equal(validId, coffee.GetProperty("id").GetInt32());
        Assert.True(coffee.TryGetProperty("name", out _));
        Assert.True(coffee.TryGetProperty("isVerified", out _));
        Assert.True(coffee.TryGetProperty("region", out _));
        Assert.True(coffee.TryGetProperty("roastery", out _));
        Assert.True(coffee.TryGetProperty("processingMethod", out _));
        Assert.True(coffee.TryGetProperty("variety", out _));
        Assert.True(coffee.TryGetProperty("createdByUserId", out _));
    }

    [Fact]
    public async Task ShouldReturnNotFoundForNonExistingId()
    {
        var response = await _client.GetAsync("/api/Coffee/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

}

public sealed class CoffeeApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["DevUser:UserId"] = "1" });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BrewDbContext>>();
            services.RemoveAll<BrewDbContext>();

            services.AddDbContext<BrewDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BrewDbContext>();
            context.Database.EnsureCreated();

            if (!context.Coffees.Any())
            {
                SeedCoffeeData(context);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static void SeedCoffeeData(BrewDbContext context)
    {
        var user = new AppUser
        {
            Id = 1,
            Login = "coffee.tester",
            Email = "coffee.tester@brewbase.local",
            PasswordHash = "test-hash",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var country = new Country
        {
            Id = 1,
            Name = "Test Country"
        };

        var region = new Region
        {
            Id = 1,
            Name = "Test Region",
            CountryId = country.Id
        };

        var roastery = new Roastery
        {
            Id = 1,
            Name = "Test Roastery"
        };

        var coffee = new Coffee
        {
            Id = 1,
            Name = "Test Coffee",
            IsVerified = true,
            RegionId = region.Id,
            RoasteryId = roastery.Id,
            CreatedByUserId = user.Id
        };

        context.AppUsers.Add(user);
        context.Countries.Add(country);
        context.Regions.Add(region);
        context.Roasteries.Add(roastery);
        context.Coffees.Add(coffee);
        context.SaveChanges();
    }
}
