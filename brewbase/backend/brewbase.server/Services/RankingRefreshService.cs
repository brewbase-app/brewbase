using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class RankingRefreshService : IRankingRefreshService
{
    private readonly BrewDbContext _context;

    public RankingRefreshService(BrewDbContext context)
    {
        _context = context;
    }

    public Task RefreshAllRankingsAsync()
    {
        return ExecuteRankingRefreshAsync("SELECT refresh_all_rankings();");
    }

    public Task RefreshUserRankingAsync()
    {
        return ExecuteRankingRefreshAsync("SELECT refresh_user_ranking();");
    }

    private async Task ExecuteRankingRefreshAsync(string sql)
    {
        var providerName = _context.Database.ProviderName ?? "";

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _context.Database.ExecuteSqlRawAsync(sql);
    }
}