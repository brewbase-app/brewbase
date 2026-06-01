using brewbase.server.Configuration;
using brewbase.server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace brewbase.server.Services;

public class RankingRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RankingRefreshOptions> _options;
    private readonly ILogger<RankingRefreshBackgroundService> _logger;

    public RankingRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<RankingRefreshOptions> options,
        ILogger<RankingRefreshBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Ranking background refresh is disabled.");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(options.IntervalMinutes, 1));

        if (options.RunOnStartup)
        {
            await RunRefreshAsync("startup", stoppingToken);
        }

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunRefreshAsync("scheduled", stoppingToken);
        }
    }

    private async Task RunRefreshAsync(string trigger, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var refreshService = scope.ServiceProvider.GetRequiredService<IRankingRefreshService>();

            if (string.Equals(trigger, "startup", StringComparison.Ordinal) &&
                !await refreshService.ShouldRefreshOnStartupAsync(cancellationToken))
            {
                _logger.LogDebug(
                    "Skipping ranking refresh on startup because snapshot is still fresh.");
                return;
            }

            var refreshed = await refreshService.TryRefreshAllRankingsAsync(cancellationToken);

            if (refreshed)
            {
                _logger.LogInformation(
                    "Ranking snapshot refreshed successfully ({Trigger}).",
                    trigger);
            }
            else
            {
                _logger.LogDebug(
                    "Ranking refresh skipped ({Trigger}).",
                    trigger);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ranking refresh failed ({Trigger}).",
                trigger);
        }
    }
}
