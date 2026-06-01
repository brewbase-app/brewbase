using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using brewbase.server.Authentication;
using Xunit;

namespace brewbase.server.Tests;

public class UserClaimsTests
{
    [Fact]
    public void GetUserId_ReadsFromStandardAndLegacyClaimTypes()
    {
        Assert.Equal(42, UserClaims.GetUserId(CreatePrincipal(
            new Claim(JwtRegisteredClaimNames.Sub, "42"))));

        Assert.Equal(7, UserClaims.GetUserId(CreatePrincipal(
            new Claim(ClaimTypes.NameIdentifier, "7"))));

        Assert.Equal(3, UserClaims.GetUserId(CreatePrincipal(
            new Claim(UserClaims.LegacyUserIdClaimType, "3"))));

        Assert.Equal(9, UserClaims.GetUserId(CreatePrincipal(
            new Claim(UserClaims.LegacyUidClaimType, "9"))));
    }

    [Fact]
    public void GetRole_ReadsFromStandardAndLegacyClaimTypes()
    {
        Assert.Equal("Admin", UserClaims.GetRole(CreatePrincipal(
            new Claim(ClaimTypes.Role, "Admin"))));

        Assert.Equal("User", UserClaims.GetRole(CreatePrincipal(
            new Claim(UserClaims.LegacyRoleClaimType, "User"))));
    }

    [Fact]
    public void Normalize_AddsStandardClaimsFromLegacyJwtClaims()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, "5"),
            new Claim(UserClaims.LegacyRoleClaimType, "Admin"),
            new Claim(UserClaims.LoginClaimType, "tester"),
        ]);

        UserClaims.Normalize(identity);

        var principal = new ClaimsPrincipal(identity);

        Assert.Equal("5", UserClaims.GetUserId(principal)?.ToString());
        Assert.Equal("Admin", UserClaims.GetRole(principal));
    }

    [Fact]
    public void IsInRole_IsCaseInsensitive()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Role, "admin"));

        Assert.True(UserClaims.IsInRole(principal, "Admin"));
        Assert.False(UserClaims.IsInRole(principal, "User"));
    }

    [Fact]
    public void Create_IncludesStandardAndLegacyClaims()
    {
        var claims = UserClaims.Create(10, "brew.tester", "Admin");

        Assert.Contains(claims, claim =>
            claim.Type == ClaimTypes.NameIdentifier && claim.Value == "10");
        Assert.Contains(claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == "10");
        Assert.Contains(claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(claims, claim =>
            claim.Type == UserClaims.LegacyRoleClaimType && claim.Value == "Admin");
        Assert.Contains(claims, claim =>
            claim.Type == UserClaims.LoginClaimType && claim.Value == "brew.tester");
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
