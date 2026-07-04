namespace StreamVault.Models.Catalogue;

public class Series : ContentItem
{
    public int Seasons { get; set; }

    public int TotalEpisodes { get; set; }

    public override string TypeDisplayName => "Series";
}
