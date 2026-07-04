using StreamVault.Models;

namespace StreamVault.Data;

public record SimulatedPlay(int ContentItemId, string Country, bool? Rating);

public interface IAnalyticsSimulator
{
    IReadOnlyList<PlayEvent> SimulateHistory(int contentId, int windowDays);

    ContentStats SimulateStats(int contentId, int recentPlays);

    SimulatedPlay NextPlay(IReadOnlyList<int> contentIds);
}
