namespace StreamVault.Models;

public class Album : ContentItem
{
    public string Artist { get; set; } = string.Empty;

    public int TrackCount { get; set; }

    public string RecordLabel { get; set; } = string.Empty;

    public override string TypeDisplayName => "Album";
}
