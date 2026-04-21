using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace brewbase.server.Authentication;

/// <summary>
/// Minimal scheme so MVC <see cref="Microsoft.AspNetCore.Mvc.ControllerBase.Forbid"/> returns 403 without redirects.
/// Does not authenticate requests.
/// </summary>
public sealed class ApiPassthroughAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ApiPassthroughAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
