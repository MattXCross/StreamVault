using StreamVault.Models;

namespace StreamVault.Data;

public static class AnalyticsSimulator
{
    private static readonly string[] Countries =
        ["GB", "US", "DE", "FR", "CA", "AU", "JP", "BR", "IN", "SE"];

    public static void EnsureSimulated(this IServiceProvider services, int retentionDays)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        if (db.ContentStats.Any())
        {
            return;
        }

        var random = new Random();

        foreach (var contentId in db.Contents.Select(c => c.Id).ToList())
        {
            var plays = SimulatePlays(contentId, retentionDays, random);
            db.PlayEvents.AddRange(plays);
            db.ContentStats.Add(SimulateStats(contentId, plays.Count, random));
        }

        db.SaveChanges();
    }

    private static List<PlayEvent> SimulatePlays(int contentId, int windowDays, Random random)
    {
        var basePlaysPerDay = random.Next(2, 16);
        var trend = (Trend)random.Next(3);
        var plays = new List<PlayEvent>();

        for (var dayOffset = 0; dayOffset < windowDays; dayOffset++)
        {
            var expected = basePlaysPerDay * TrendFactor(trend, dayOffset, windowDays) * (0.7 + random.NextDouble() * 0.6);

            for (var i = 0; i < (int)Math.Round(expected); i++)
            {
                plays.Add(new PlayEvent
                {
                    ContentItemId = contentId,
                    PlayedAt = DateTime.UtcNow.Date.AddDays(-dayOffset).AddSeconds(random.Next(86400)),
                    Country = Countries[random.Next(Countries.Length)]
                });
            }
        }

        return plays;
    }

    private static ContentStats SimulateStats(int contentId, int recentPlays, Random random)
    {
        var lifetimePlays = recentPlays + random.Next(recentPlays * 2, recentPlays * 8 + 1);
        var ratingCount = (int)Math.Round(lifetimePlays * (0.05 + random.NextDouble() * 0.10));
        var thumbsUp = (int)Math.Round(ratingCount * (0.5 + random.NextDouble() * 0.47));

        return new ContentStats
        {
            ContentItemId = contentId,
            TotalPlays = lifetimePlays,
            ThumbsUp = thumbsUp,
            ThumbsDown = ratingCount - thumbsUp
        };
    }

    private static double TrendFactor(Trend trend, int dayOffset, int windowDays)
    {
        var span = Math.Max(1, windowDays - 1);
        return trend switch
        {
            Trend.Declining => 0.3 + 0.7 * (dayOffset / (double)span),
            Trend.Rising => 0.3 + 0.7 * ((span - dayOffset) / (double)span),
            _ => 1.0
        };
    }

    private enum Trend
    {
        Declining,
        Steady,
        Rising
    }
}
