using StreamVault.Models;

namespace StreamVault.Data;

public interface IAnalyticsService
{
    Task<ContentStats?> GetStatsAsync(int contentItemId);

    Task<IReadOnlyList<DailyPlayCount>> GetPlaysOverTimeAsync(int contentItemId, int days = 30);

    Task<IReadOnlyList<CountryPlayCount>> GetCountryBreakdownAsync(int contentItemId, int days = 30);

    Task RecordPlayAsync(int contentItemId, string country);

    Task RecordRatingAsync(int contentItemId, bool positive);
}
