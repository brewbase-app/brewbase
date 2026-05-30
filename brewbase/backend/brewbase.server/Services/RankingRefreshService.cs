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

    public async Task RefreshAllRankingsAsync()
    {
        var providerName = _context.Database.ProviderName ?? "";

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _context.Database.ExecuteSqlRawAsync("SELECT refresh_all_rankings();");
    }
}