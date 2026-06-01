using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using brewbase.server.Authentication;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using brewbase.server.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace brewbase.server.Tests;

public class AuthJwtClaimsTests : IDisposable
{
    private readonly CoffeeApiFactory _factory;

    public AuthJwtClaimsTests()
    {
        _factory = new CoffeeApiFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public void GenerateJwt_IncludesStandardRoleAndUserIdClaims()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var token = authService.GenerateJwt(new AppUser
        {
            Id = 42,
            Login = "jwt.tester",
            Role = "Admin",
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "42");

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == "42");
    }

    [Fact]
    public async Task JwtBearerToken_AllowsAdminAuthorizationOnProtectedEndpoint()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var token = authService.GenerateJwt(new AppUser
        {
            Id = 2,
            Login = "admin.tester",
            Role = "Admin",
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/Ranking/refresh", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AuthMe_ReturnsRoleFromGeneratedToken()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var token = authService.GenerateJwt(new AppUser
        {
            Id = 1,
            Login = "coffee.tester",
            Role = "User",
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/Auth/me");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("userId").GetInt32());
        Assert.Equal("coffee.tester", root.GetProperty("login").GetString());
        Assert.Equal("User", root.GetProperty("role").GetString());
    }

    [Fact]
    public async Task JwtBearerToken_WithLegacyRoleClaimOnly_StillAuthorizesAdmin()
    {
        var token = CreateLegacyRoleOnlyToken(
            userId: 2,
            role: "Admin",
            key: "TEST_SECRET_KEY_12345678901234567890",
            issuer: "test",
            audience: "test");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/Ranking/refresh", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static string CreateLegacyRoleOnlyToken(
        int userId,
        string role,
        string key,
        string issuer,
        string audience)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("login", "legacy.tester"),
            new Claim("role", role),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
