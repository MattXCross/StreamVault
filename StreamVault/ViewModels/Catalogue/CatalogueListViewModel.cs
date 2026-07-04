using StreamVault.Data;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Analytics;

namespace StreamVault.ViewModels.Catalogue;

public class CatalogueListViewModel
{
    public required PagedResult<ContentItem> Results { get; init; }

    public string? TypeFilter { get; init; }

    public string? SearchTerm { get; init; }

    public int? ExpandedId { get; init; }

    public ContentAnalyticsViewModel? AnalyticsPanel { get; init; }
}
