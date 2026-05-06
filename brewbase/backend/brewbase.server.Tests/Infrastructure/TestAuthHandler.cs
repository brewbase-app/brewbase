using System.Security.Claims;
using System.Text.Encodings.Web;
using brewbase.server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace brewbase.server.Tests.Infrastructure;

/// <summary>
/// Test host only: satisfies [Authorize] when <see cref="CurrentUserProvider.DevUserIdHeaderName"/> is set.
/// Claims mirror a minimal JWT so <see cref="ICurrentUserProvider"/> resolves the same user id as dev header flow.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers[CurrentUserProvider.DevUserIdHeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(header) || !int.TryParse(header, out var userId) || userId <= 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("login", "test"),
            new Claim("role", "User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
