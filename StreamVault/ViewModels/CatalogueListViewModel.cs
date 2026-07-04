using StreamVault.Data;
using StreamVault.Models;

namespace StreamVault.ViewModels;

public class CatalogueListViewModel
{
    public required PagedResult<ContentItem> Results { get; init; }

    public string? TypeFilter { get; init; }

    public string? SearchTerm { get; init; }
}
