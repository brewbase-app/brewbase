using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace brewbase.server.Tests.Postgres.Infrastructure;

internal sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PostgresWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Key"] = "TEST_SECRET_KEY_12345678901234567890",
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test",
                ["DevUser:UserId"] = "0"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(PostgresTestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, PostgresTestAuthHandler>(
                    PostgresTestAuthHandler.SchemeName,
                    _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = PostgresTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = PostgresTestAuthHandler.SchemeName;
            });
        });
    }
}
