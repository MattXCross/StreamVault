using System.ComponentModel.DataAnnotations;
using StreamVault.Models;

namespace StreamVault.ViewModels;

public class MovieFormViewModel : ContentFormViewModel<Movie>
{
    [Required(ErrorMessage = "The Duration field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive number of minutes.")]
    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; }

    [Required]
    [StringLength(100)]
    public string? Director { get; set; }

    public override void ApplyTo(Movie entity)
    {
        base.ApplyTo(entity);
        entity.DurationMinutes = DurationMinutes.GetValueOrDefault();
        entity.Director = Director ?? string.Empty;
    }

    public override void LoadFrom(Movie entity)
    {
        base.LoadFrom(entity);
        DurationMinutes = entity.DurationMinutes;
        Director = entity.Director;
    }
}
