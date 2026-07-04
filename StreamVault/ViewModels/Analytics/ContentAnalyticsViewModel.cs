using StreamVault.Data.Analytics;
using StreamVault.Models.Analytics;

namespace StreamVault.ViewModels.Analytics;

public class ContentAnalyticsViewModel
{
    public static readonly IReadOnlyList<int> WindowOptions = [7, 14, 30];

    public required int ContentId { get; init; }

    public required int WindowDays { get; init; }

    public ContentStats? Stats { get; init; }

    public required IReadOnlyList<DailyPlayCount> PlaysOverTime { get; init; }

    public required IReadOnlyList<CountryPlayCount> Countries { get; init; }

    public bool HasStats => Stats is not null;

    public int WindowPlays => PlaysOverTime.Sum(d => d.Count);
}
