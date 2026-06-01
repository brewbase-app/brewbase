using System.Security.Claims;
using System.Text.Encodings.Web;
using brewbase.server.Authentication;
using brewbase.server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace brewbase.server.Tests.Postgres.Infrastructure;

internal sealed class PostgresTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "PostgresTest";

    public PostgresTestAuthHandler(
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

        var identity = new ClaimsIdentity(
            UserClaims.Create(userId, "postgres.tester", "User"),
            Scheme.Name);
        UserClaims.Normalize(identity);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
