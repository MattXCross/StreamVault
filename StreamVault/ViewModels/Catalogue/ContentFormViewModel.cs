using System.ComponentModel.DataAnnotations;
using StreamVault.Models.Catalogue;

namespace StreamVault.ViewModels.Catalogue;

public abstract class ContentFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "The Release date field is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Release date")]
    public DateOnly? ReleaseDate { get; set; }

    [Required(ErrorMessage = "The Age rating field is required.")]
    [Display(Name = "Age rating")]
    public AgeRating? AgeRating { get; set; }

    [Required]
    [StringLength(100)]
    public string? Genre { get; set; }
}

public abstract class ContentFormViewModel<TContent> : ContentFormViewModel
    where TContent : ContentItem
{
    public virtual void ApplyTo(TContent entity)
    {
        entity.Title = Title ?? string.Empty;
        entity.Description = Description;
        entity.ReleaseDate = ReleaseDate.GetValueOrDefault();
        entity.AgeRating = AgeRating.GetValueOrDefault();
        entity.Genre = Genre ?? string.Empty;
    }

    public virtual void LoadFrom(TContent entity)
    {
        Id = entity.Id;
        Title = entity.Title;
        Description = entity.Description;
        ReleaseDate = entity.ReleaseDate;
        AgeRating = entity.AgeRating;
        Genre = entity.Genre;
    }
}
