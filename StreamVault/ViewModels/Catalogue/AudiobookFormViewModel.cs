using System.ComponentModel.DataAnnotations;
using StreamVault.Models.Catalogue;

namespace StreamVault.ViewModels.Catalogue;

public class AudiobookFormViewModel : ContentFormViewModel<Audiobook>
{
    [Required]
    [StringLength(100)]
    public string? Author { get; set; }

    [Required]
    [StringLength(100)]
    public string? Narrator { get; set; }

    [Required(ErrorMessage = "The Duration field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive number of minutes.")]
    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; }

    public override void ApplyTo(Audiobook entity)
    {
        base.ApplyTo(entity);
        entity.Author = Author ?? string.Empty;
        entity.Narrator = Narrator ?? string.Empty;
        entity.DurationMinutes = DurationMinutes.GetValueOrDefault();
    }

    public override void LoadFrom(Audiobook entity)
    {
        base.LoadFrom(entity);
        Author = entity.Author;
        Narrator = entity.Narrator;
        DurationMinutes = entity.DurationMinutes;
    }
}
