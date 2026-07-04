using System.ComponentModel.DataAnnotations;

namespace StreamVault.Models;

public enum AgeRating
{
    [Display(Name = "U")]
    Universal,

    [Display(Name = "PG")]
    ParentalGuidance,

    [Display(Name = "12")]
    Twelve,

    [Display(Name = "15")]
    Fifteen,

    [Display(Name = "18")]
    Eighteen
}
