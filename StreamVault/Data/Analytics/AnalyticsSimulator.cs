using StreamVault.Models.Analytics;

namespace StreamVault.Data.Analytics;

public record SimulatedPlay(int ContentItemId, string Country, bool? Rating);

public class AnalyticsSimulator
{
    private const double RatingChance = 0.1;
    private const double PositiveRatingChance = 0.75;

    private static readonly string[] Countries =
        ["GB", "US", "DE", "FR", "CA", "AU", "JP", "BR", "IN", "SE"];

    private readonly Random random;

    public AnalyticsSimulator() : this(new Random())
    {
    }

    internal AnalyticsSimulator(Random random)
    {
        this.random = random;
    }

    public IReadOnlyList<PlayEvent> SimulateHistory(int contentId, int windowDays)
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

    public ContentStats SimulateStats(int contentId, int recentPlays)
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

    public SimulatedPlay NextPlay(IReadOnlyList<int> contentIds)
    {
        var contentId = contentIds[random.Next(contentIds.Count)];
        var country = Countries[random.Next(Countries.Length)];
        bool? rating = random.NextDouble() < RatingChance
            ? random.NextDouble() < PositiveRatingChance
            : null;

        return new SimulatedPlay(contentId, country, rating);
    }

    internal static double TrendFactor(Trend trend, int dayOffset, int windowDays)
    {
        var span = Math.Max(1, windowDays - 1);
        return trend switch
        {
            Trend.Declining => 0.3 + 0.7 * (dayOffset / (double)span),
            Trend.Rising => 0.3 + 0.7 * ((span - dayOffset) / (double)span),
            _ => 1.0
        };
    }

    internal enum Trend
    {
        Declining,
        Steady,
        Rising
    }
}
