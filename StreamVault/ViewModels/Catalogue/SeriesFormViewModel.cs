using System.ComponentModel.DataAnnotations;
using StreamVault.Models.Catalogue;

namespace StreamVault.ViewModels.Catalogue;

public class SeriesFormViewModel : ContentFormViewModel<Series>, IValidatableObject
{
    [Required(ErrorMessage = "The Seasons field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seasons must be a positive number.")]
    public int? Seasons { get; set; }

    [Required(ErrorMessage = "The Total episodes field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Total episodes must be a positive number.")]
    [Display(Name = "Total episodes")]
    public int? TotalEpisodes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TotalEpisodes < Seasons)
        {
            yield return new ValidationResult(
                "Total episodes cannot be fewer than the number of seasons.",
                [nameof(TotalEpisodes)]);
        }
    }

    public override void ApplyTo(Series entity)
    {
        base.ApplyTo(entity);
        entity.Seasons = Seasons.GetValueOrDefault();
        entity.TotalEpisodes = TotalEpisodes.GetValueOrDefault();
    }

    public override void LoadFrom(Series entity)
    {
        base.LoadFrom(entity);
        Seasons = entity.Seasons;
        TotalEpisodes = entity.TotalEpisodes;
    }
}
