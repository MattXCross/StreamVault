namespace StreamVault.Models;

public class Audiobook : ContentItem
{
    public string Author { get; set; } = string.Empty;

    public string Narrator { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public override string TypeDisplayName => "Audiobook";
}
