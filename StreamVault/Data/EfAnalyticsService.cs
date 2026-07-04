using Microsoft.EntityFrameworkCore;
using StreamVault.Models;

namespace StreamVault.Data;

public class EfAnalyticsService(CatalogueDbContext db) : IAnalyticsService
{
    public async Task<ContentStats?> GetStatsAsync(int contentItemId)
    {
        return await db.ContentStats.SingleOrDefaultAsync(s => s.ContentItemId == contentItemId);
    }

    public async Task<IReadOnlyList<DailyPlayCount>> GetPlaysOverTimeAsync(int contentItemId, int days = 30)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(days - 1));
        var cutoff = startDate.ToDateTime(TimeOnly.MinValue);

        var dailyCounts = await db.PlayEvents
            .Where(e => e.ContentItemId == contentItemId && e.PlayedAt >= cutoff)
            .GroupBy(e => e.PlayedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync();

        var countsByDate = dailyCounts
            .ToDictionary(x => DateOnly.FromDateTime(x.Day), x => x.Count);

        return Enumerable.Range(0, days)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new DailyPlayCount(date, countsByDate.GetValueOrDefault(date)))
            .ToList();
    }

    public async Task<IReadOnlyList<CountryPlayCount>> GetCountryBreakdownAsync(int contentItemId, int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var grouped = await db.PlayEvents
            .Where(e => e.ContentItemId == contentItemId && e.PlayedAt >= cutoff)
            .GroupBy(e => e.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        return grouped.Select(g => new CountryPlayCount(g.Country, g.Count)).ToList();
    }

    public async Task RecordPlayAsync(int contentItemId, string country)
    {
        db.PlayEvents.Add(new PlayEvent
        {
            ContentItemId = contentItemId,
            PlayedAt = DateTime.UtcNow,
            Country = country
        });

        var stats = await GetOrCreateStatsAsync(contentItemId);
        stats.TotalPlays++;

        await db.SaveChangesAsync();
    }

    public async Task RecordRatingAsync(int contentItemId, bool positive)
    {
        var stats = await GetOrCreateStatsAsync(contentItemId);
        if (positive)
        {
            stats.ThumbsUp++;
        }
        else
        {
            stats.ThumbsDown++;
        }

        await db.SaveChangesAsync();
    }

    private async Task<ContentStats> GetOrCreateStatsAsync(int contentItemId)
    {
        var stats = await db.ContentStats.SingleOrDefaultAsync(s => s.ContentItemId == contentItemId);
        if (stats is null)
        {
            stats = new ContentStats { ContentItemId = contentItemId };
            db.ContentStats.Add(stats);
        }

        return stats;
    }
}
