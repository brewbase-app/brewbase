namespace brewbase.server.Services.Interfaces;

public interface IRankingRefreshService
{
    Task RefreshAllRankingsAsync();

    Task RefreshUserRankingAsync();
}