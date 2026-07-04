using System.ComponentModel.DataAnnotations;
using StreamVault.Models;

namespace StreamVault.ViewModels;

public class AlbumFormViewModel : ContentFormViewModel<Album>
{
    [Required]
    [StringLength(100)]
    public string? Artist { get; set; }

    [Required(ErrorMessage = "The Track count field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Track count must be a positive number.")]
    [Display(Name = "Track count")]
    public int? TrackCount { get; set; }

    [Required(ErrorMessage = "The Record label field is required.")]
    [StringLength(100)]
    [Display(Name = "Record label")]
    public string? RecordLabel { get; set; }

    public override void ApplyTo(Album entity)
    {
        base.ApplyTo(entity);
        entity.Artist = Artist ?? string.Empty;
        entity.TrackCount = TrackCount.GetValueOrDefault();
        entity.RecordLabel = RecordLabel ?? string.Empty;
    }

    public override void LoadFrom(Album entity)
    {
        base.LoadFrom(entity);
        Artist = entity.Artist;
        TrackCount = entity.TrackCount;
        RecordLabel = entity.RecordLabel;
    }
}
