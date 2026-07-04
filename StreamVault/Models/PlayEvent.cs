namespace StreamVault.Models;

public class PlayEvent
{
    public int Id { get; set; }

    public int ContentItemId { get; set; }

    public DateTime PlayedAt { get; set; }

    public string Country { get; set; } = string.Empty;
}
