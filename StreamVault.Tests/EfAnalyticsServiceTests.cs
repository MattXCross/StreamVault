using StreamVault.Data.Analytics;
using StreamVault.Models.Analytics;
using StreamVault.Models.Catalogue;

namespace StreamVault.Tests;

public class EfAnalyticsServiceTests : IDisposable
{
    private readonly SqliteTestDb TestDb = new();
    private readonly EfAnalyticsService Service;

    public EfAnalyticsServiceTests()
    {
        Service = new EfAnalyticsService(TestDb.Context);
    }

    public void Dispose() => TestDb.Dispose();

    private int SeedTestMovie()
    {
        var movie = new Movie
        {
            Title = "Iron Tide",
            ReleaseDate = new DateOnly(2019, 11, 8),
            AgeRating = AgeRating.Twelve,
            Genre = "Action",
            Director = "Kwame Osei",
            DurationMinutes = 117
        };
        TestDb.Context.Contents.Add(movie);
        TestDb.Context.SaveChanges();
        return movie.Id;
    }

    private void AddTestPlay(int contentId, DateTime playedAt, string country = "GB")
    {
        TestDb.Context.PlayEvents.Add(new PlayEvent
        {
            ContentItemId = contentId,
            PlayedAt = playedAt,
            Country = country
        });
    }

    [Fact]
    public async Task PlaysOverTimeZeroFillsWindow()
    {
        var contentId = SeedTestMovie();
        var today = DateTime.UtcNow.Date;
        AddTestPlay(contentId, today.AddHours(1));
        AddTestPlay(contentId, today.AddHours(13));
        AddTestPlay(contentId, today.AddDays(-2).AddHours(5));
        AddTestPlay(contentId, today.AddDays(-7).AddHours(5));
        TestDb.Context.SaveChanges();

        var series = await Service.GetPlaysOverTimeAsync(contentId, days: 7);

        Assert.Equal(7, series.Count);
        Assert.Equal(DateOnly.FromDateTime(today.AddDays(-6)), series[0].Date);
        Assert.Equal(DateOnly.FromDateTime(today), series[^1].Date);
        Assert.Equal(2, series[^1].Count);
        Assert.Equal(1, series[4].Count);
        Assert.Equal(3, series.Sum(d => d.Count));
        for (var i = 1; i < series.Count; i++)
        {
            Assert.Equal(series[i - 1].Date.AddDays(1), series[i].Date);
        }
    }

    [Fact]
    public async Task CountryBreakdownOrdersByPlayCount()
    {
        var contentId = SeedTestMovie();
        var today = DateTime.UtcNow.Date;
        AddTestPlay(contentId, today.AddHours(1), "GB");
        AddTestPlay(contentId, today.AddHours(2), "GB");
        AddTestPlay(contentId, today.AddDays(-1), "GB");
        AddTestPlay(contentId, today.AddHours(3), "US");
        AddTestPlay(contentId, today.AddDays(-40), "FR");
        TestDb.Context.SaveChanges();

        var countries = await Service.GetCountryBreakdownAsync(contentId, days: 30);

        Assert.Equal(2, countries.Count);
        Assert.Equal(new CountryPlayCount("GB", 3), countries[0]);
        Assert.Equal(new CountryPlayCount("US", 1), countries[1]);
    }

    [Fact]
    public async Task RecordPlayIncrementsStats()
    {
        var contentId = SeedTestMovie();

        Assert.Null(await Service.GetStatsAsync(contentId));

        await Service.RecordPlayAsync(contentId, "GB");
        await Service.RecordPlayAsync(contentId, "US");

        var stats = await Service.GetStatsAsync(contentId);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalPlays);
        Assert.Equal(2, TestDb.Context.PlayEvents.Count(e => e.ContentItemId == contentId));
    }

    [Fact]
    public async Task RecordRatingIncrementsThumbsUpOrDown()
    {
        var contentId = SeedTestMovie();

        await Service.RecordRatingAsync(contentId, positive: true);
        await Service.RecordRatingAsync(contentId, positive: true);
        await Service.RecordRatingAsync(contentId, positive: false);

        var stats = await Service.GetStatsAsync(contentId);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.ThumbsUp);
        Assert.Equal(1, stats.ThumbsDown);
    }
}
