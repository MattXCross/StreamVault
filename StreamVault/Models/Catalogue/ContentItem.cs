namespace StreamVault.Models.Catalogue;

public abstract class ContentItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public AgeRating AgeRating { get; set; }

    public string Genre { get; set; } = string.Empty;
    
    public abstract string TypeDisplayName { get; }
}
