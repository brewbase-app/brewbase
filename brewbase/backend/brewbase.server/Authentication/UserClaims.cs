using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using brewbase.server.Models;

namespace brewbase.server.Authentication;

public static class UserClaims
{
    public const string LoginClaimType = "login";
    public const string LegacyRoleClaimType = "role";
    public const string LegacyUserIdClaimType = "user_id";
    public const string LegacyUidClaimType = "uid";

    private static readonly string[] UserIdClaimTypes =
    {
        ClaimTypes.NameIdentifier,
        JwtRegisteredClaimNames.Sub,
        LegacyUserIdClaimType,
        LegacyUidClaimType,
    };

    private static readonly string[] RoleClaimTypes =
    {
        ClaimTypes.Role,
        LegacyRoleClaimType,
    };

    public static IReadOnlyList<Claim> Create(AppUser user)
    {
        return Create(user.Id, user.Login, user.Role);
    }

    public static IReadOnlyList<Claim> Create(int userId, string login, string role)
    {
        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(LoginClaimType, login),
            new Claim(ClaimTypes.Role, role),
            new Claim(LegacyRoleClaimType, role),
        ];
    }

    public static void Normalize(ClaimsIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.FindFirst(ClaimTypes.Role) == null)
        {
            var legacyRole = identity.FindFirst(LegacyRoleClaimType);
            if (legacyRole != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, legacyRole.Value));
            }
        }

        if (identity.FindFirst(ClaimTypes.NameIdentifier) == null)
        {
            var subject = identity.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? identity.FindFirst(LegacyUserIdClaimType)
                ?? identity.FindFirst(LegacyUidClaimType);

            if (subject != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subject.Value));
            }
        }
    }

    public static int? GetUserId(ClaimsPrincipal? user)
    {
        var rawValue = GetFirstValue(user, UserIdClaimTypes);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return int.TryParse(rawValue, out var userId) && userId > 0
            ? userId
            : null;
    }

    public static string? GetLogin(ClaimsPrincipal? user)
    {
        return GetFirstValue(user, LoginClaimType);
    }

    public static string? GetRole(ClaimsPrincipal? user)
    {
        return GetFirstValue(user, RoleClaimTypes);
    }

    public static bool IsInRole(ClaimsPrincipal? user, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var actualRole = GetRole(user);
        return actualRole != null &&
               string.Equals(actualRole, role, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetFirstValue(ClaimsPrincipal? user, params string[] claimTypes)
    {
        if (user == null)
        {
            return null;
        }

        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
