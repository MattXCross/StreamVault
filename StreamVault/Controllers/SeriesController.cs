using StreamVault.Data;
using StreamVault.Models;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class SeriesController(ICatalogueRepository repository)
    : ContentEntryController<Series, SeriesFormViewModel>(repository);
