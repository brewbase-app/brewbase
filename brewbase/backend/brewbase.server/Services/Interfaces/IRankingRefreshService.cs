namespace brewbase.server.Services.Interfaces;

public interface IRankingRefreshService
{
    Task RefreshAllRankingsAsync(CancellationToken cancellationToken = default);

    Task RefreshUserRankingAsync(CancellationToken cancellationToken = default);

    Task<bool> TryRefreshAllRankingsAsync(CancellationToken cancellationToken = default);

    Task<bool> ShouldRefreshOnStartupAsync(CancellationToken cancellationToken = default);
}
