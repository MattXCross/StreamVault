using StreamVault.Data;

namespace StreamVault.Tests;

public class AnalyticsSimulatorTests
{
    private readonly AnalyticsSimulator Simulator = new();

    [Fact]
    public void SimulateHistoryStaysWithinWindow()
    {
        var plays = Simulator.SimulateHistory(contentId: 1, windowDays: 30);

        Assert.NotEmpty(plays);
        var oldestAllowed = DateTime.UtcNow.Date.AddDays(-29);
        var newestAllowed = DateTime.UtcNow.Date.AddDays(1);
        Assert.All(plays, p =>
        {
            Assert.Equal(1, p.ContentItemId);
            Assert.InRange(p.PlayedAt, oldestAllowed, newestAllowed);
            Assert.Equal(2, p.Country.Length);
        });
    }

    [Fact]
    public void SimulateStatsTotalsAreConsistent()
    {
        var stats = Simulator.SimulateStats(contentId: 5, recentPlays: 100);

        Assert.Equal(5, stats.ContentItemId);
        Assert.True(stats.TotalPlays >= 300);
        Assert.InRange(stats.TotalRatings, 0, stats.TotalPlays);
        Assert.True(stats.ThumbsUp >= 0);
        Assert.True(stats.ThumbsDown >= 0);
    }
}
