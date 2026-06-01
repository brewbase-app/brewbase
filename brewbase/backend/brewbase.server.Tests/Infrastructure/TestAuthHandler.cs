using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using brewbase.server.Authentication;
using brewbase.server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

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

        var identity = new ClaimsIdentity(UserClaims.Create(userId, "test", "User"), Scheme.Name);
        UserClaims.Normalize(identity);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
