using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace brewbase.server.Services;

/// <summary>
/// JWT claims first; in Development only, <see cref="DevUserConfigSection"/> and <c>X-Dev-User-Id</c>.
/// Do not rely on Development-only fallbacks in production.
/// </summary>
public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public const string DevUserIdHeaderName = "X-Dev-User-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CurrentUserProvider(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _environment = environment;
    }

    public int? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
            var fromClaims = TryParseUserIdFromClaims(httpContext.User);
            if (fromClaims.HasValue)
            {
                return fromClaims;
            }
        }

        if (!_environment.IsDevelopment())
        {
            return null;
        }

        var configured = _configuration.GetValue<int?>($"{DevUserConfigSection.SectionName}:UserId");
        if (configured is > 0)
        {
            return configured;
        }

        var headerValue = httpContext?.Request.Headers[DevUserIdHeaderName].FirstOrDefault();
        if (int.TryParse(headerValue, out var fromHeader) && fromHeader > 0)
        {
            return fromHeader;
        }

        return null;
    }

    private static int? TryParseUserIdFromClaims(ClaimsPrincipal user)
    {
        var candidates = new[]
        {
            user.FindFirstValue("sub"),
            user.FindFirstValue(ClaimTypes.NameIdentifier),
            user.FindFirstValue("user_id")
        };

        foreach (var value in candidates)
        {
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var id) && id > 0)
            {
                return id;
            }
        }

        return null;
    }
}

/// <summary>
/// Configuration keys under <c>DevUser</c> for local development when real auth is not wired yet.
/// </summary>
public static class DevUserConfigSection
{
    public const string SectionName = "DevUser";
}
