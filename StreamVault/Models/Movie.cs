namespace StreamVault.Models;

public class Movie : ContentItem
{
    public int DurationMinutes { get; set; }

    public string Director { get; set; } = string.Empty;

    public override string TypeDisplayName => "Movie";
}
