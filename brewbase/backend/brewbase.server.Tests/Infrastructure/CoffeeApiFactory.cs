using brewbase.server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
    
    public HttpClient CreateAuthenticatedClient(int userId = 1)
    {
        var client = CreateClient();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("user_id", userId.ToString()),
            new Claim("login", "coffee.tester"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("role", "User"),
            new Claim("uid", userId.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("TEST_SECRET_KEY_12345678901234567890"));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenValue);

        return client;
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

        var northRegion = new Region
        {
            Id = 1,
            Name = "North Region",
            CountryId = country.Id
        };

        var southRegion = new Region
        {
            Id = 2,
            Name = "South Region",
            CountryId = country.Id
        };

        var roasteryOne = new Roastery
        {
            Id = 1,
            Name = "Roastery One"
        };

        var roasteryTwo = new Roastery
        {
            Id = 2,
            Name = "Roastery Two"
        };

        var brewingMethod = new BrewingMethod
        {
            Id = 1,
            Name = "V60",
            Description = "Pour-over brewing method for tests."
        };

        var coffee1 = new Coffee
        {
            Id = 1,
            Name = "Alpha Coffee",
            IsVerified = true,
            RegionId = northRegion.Id,
            RoasteryId = roasteryOne.Id,
            CreatedByUserId = user.Id
        };

        var coffee2 = new Coffee
        {
            Id = 2,
            Name = "Beta Coffee",
            IsVerified = true,
            RegionId = northRegion.Id,
            RoasteryId = roasteryTwo.Id,
            CreatedByUserId = user.Id
        };

        var coffee3 = new Coffee
        {
            Id = 3,
            Name = "Zulu Coffee",
            IsVerified = false,
            RegionId = southRegion.Id,
            RoasteryId = roasteryOne.Id,
            CreatedByUserId = user.Id
        };

        context.AppUsers.Add(user);
        context.Countries.Add(country);
        context.Regions.AddRange(northRegion, southRegion);
        context.Roasteries.AddRange(roasteryOne, roasteryTwo);
        context.BrewingMethods.Add(brewingMethod);
        context.Coffees.AddRange(coffee1, coffee2, coffee3);
        context.SaveChanges();
    }
}
