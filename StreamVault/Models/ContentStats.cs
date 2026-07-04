using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Models;

public class ContentStats
{
    public int Id { get; set; }

    public int ContentItemId { get; set; }

    public int TotalPlays { get; set; }

    public int ThumbsUp { get; set; }

    public int ThumbsDown { get; set; }

    [NotMapped]
    public int TotalRatings => ThumbsUp + ThumbsDown;

    [NotMapped]
    public double PositiveRatingPercent => TotalRatings == 0 ? 0 : (double)ThumbsUp / TotalRatings * 100;
}
