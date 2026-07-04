using StreamVault.Models;

namespace StreamVault.ViewModels;

public record ContentTypeInfo(string DisplayName, string ControllerName);

/// <summary>
/// The single place that knows which controller serves each content type,
/// so views can build per-type links without checking concrete types.
/// </summary>
public static class ContentTypes
{
    public static readonly IReadOnlyList<ContentTypeInfo> All =
    [
        new("Movie", "Movies"),
        new("Series", "Series"),
        new("Audiobook", "Audiobooks"),
        new("Album", "Albums")
    ];

    public static string ControllerFor(ContentItem item) =>
        All.Single(t => t.DisplayName == item.TypeDisplayName).ControllerName;
}
