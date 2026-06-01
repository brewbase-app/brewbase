using brewbase.server.Configuration;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace brewbase.server.Services;

public class RankingRefreshService : IRankingRefreshService
{
    private const long AdvisoryLockKey = 867530901;

    private readonly BrewDbContext _context;
    private readonly IOptions<RankingRefreshOptions> _options;
    private readonly ILogger<RankingRefreshService> _logger;

    public RankingRefreshService(
        BrewDbContext context,
        IOptions<RankingRefreshOptions> options,
        ILogger<RankingRefreshService> logger)
    {
        _context = context;
        _options = options;
        _logger = logger;
    }

    public async Task RefreshAllRankingsAsync(CancellationToken cancellationToken = default)
    {
        await TryRefreshAllRankingsAsync(cancellationToken);
    }

    public async Task RefreshUserRankingAsync(CancellationToken cancellationToken = default)
    {
        if (IsSqlite())
        {
            return;
        }

        await TryExecuteRankingRefreshAsync(
            "SELECT refresh_user_ranking();",
            cancellationToken,
            requireLock: true);
    }

    public async Task<bool> TryRefreshAllRankingsAsync(CancellationToken cancellationToken = default)
    {
        if (IsSqlite())
        {
            return true;
        }

        return await TryExecuteRankingRefreshAsync(
            "SELECT refresh_all_rankings();",
            cancellationToken,
            requireLock: true);
    }

    public async Task<bool> ShouldRefreshOnStartupAsync(CancellationToken cancellationToken = default)
    {
        if (IsSqlite())
        {
            return false;
        }

        if (!await _context.UserRankings.AsNoTracking().AnyAsync(cancellationToken))
        {
            return true;
        }

        var latestRefresh = await _context.UserRankings
            .AsNoTracking()
            .MaxAsync(ranking => (DateTime?)ranking.RefreshedAt, cancellationToken);

        if (latestRefresh == null)
        {
            return true;
        }

        var staleAfter = TimeSpan.FromHours(Math.Max(_options.Value.StaleAfterHours, 1));
        var age = DateTime.UtcNow - DateTime.SpecifyKind(latestRefresh.Value, DateTimeKind.Utc);

        return age > staleAfter;
    }

    private async Task<bool> TryExecuteRankingRefreshAsync(
        string sql,
        CancellationToken cancellationToken,
        bool requireLock = false)
    {
        if (IsSqlite())
        {
            return false;
        }

        var lockAcquired = false;

        try
        {
            if (requireLock)
            {
                lockAcquired = await TryAcquireAdvisoryLockAsync(cancellationToken);

                if (!lockAcquired)
                {
                    _logger.LogDebug(
                        "Ranking refresh skipped because another refresh is in progress.");
                    return false;
                }
            }

            await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ranking refresh SQL failed.");
            throw;
        }
        finally
        {
            if (lockAcquired)
            {
                await ReleaseAdvisoryLockAsync(cancellationToken);
            }
        }
    }

    private async Task<bool> TryAcquireAdvisoryLockAsync(CancellationToken cancellationToken)
    {
        var result = await _context.Database
            .SqlQueryRaw<bool>($"SELECT pg_try_advisory_lock({AdvisoryLockKey}) AS \"Value\"")
            .FirstAsync(cancellationToken);

        return result;
    }

    private Task ReleaseAdvisoryLockAsync(CancellationToken cancellationToken)
    {
        return _context.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_unlock({AdvisoryLockKey})",
            cancellationToken);
    }

    private bool IsSqlite()
    {
        var providerName = _context.Database.ProviderName ?? "";
        return providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
    }
}
