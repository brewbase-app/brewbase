using brewbase.server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace brewbase.server.Tests.Infrastructure;

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
                new Dictionary<string, string?>
                {
                    ["DevUser:UserId"] = "1",
                    ["Jwt:Key"] = "TEST_SECRET_KEY_12345678901234567890",
                    ["Jwt:Issuer"] = "test",
                    ["Jwt:Audience"] = "test"
                });
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

        var brewingMethod = new BrewingMethod
        {
            Id = 1,
            Name = "V60",
            Description = "Pour-over brewing method for tests."
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
        context.BrewingMethods.Add(brewingMethod);
        context.Coffees.Add(coffee);
        context.SaveChanges();
    }
}
