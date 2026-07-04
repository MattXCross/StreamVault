namespace StreamVault.Models;

/// <summary>
/// Base class for everything in the catalogue. Stored as a single table
/// (table-per-hierarchy) with a discriminator column per content type.
/// </summary>
public abstract class ContentItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public AgeRating AgeRating { get; set; }

    public string Genre { get; set; } = string.Empty;

    /// <summary>Human-readable type name shown in the catalogue list.</summary>
    public abstract string TypeDisplayName { get; }
}
