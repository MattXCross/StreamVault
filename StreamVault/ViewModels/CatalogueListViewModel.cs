using StreamVault.Models;

namespace StreamVault.ViewModels;

public class CatalogueListViewModel
{
    public required IReadOnlyList<ContentItem> Items { get; init; }

    public string? TypeFilter { get; init; }

    public string? SearchTerm { get; init; }
}
