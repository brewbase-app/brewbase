namespace brewbase.server.Services;

/// <summary>
/// Resolves the application user id (<c>app_user.id</c>) for the current HTTP request.
/// Implementations may use real authentication (claims) or temporary development configuration.
/// </summary>
public interface ICurrentUserProvider
{
    /// <returns>The signed-in user's id, or <c>null</c> when the caller is anonymous or resolution failed.</returns>
    int? GetUserId();
}
