namespace brewbase.server.Configuration;

public class RankingRefreshOptions
{
    public const string SectionName = "RankingRefresh";

    public bool Enabled { get; set; }

    public int IntervalMinutes { get; set; } = 60;

    public bool RunOnStartup { get; set; } = true;

    public int StaleAfterHours { get; set; } = 2;

    public string? Secret { get; set; }
}
