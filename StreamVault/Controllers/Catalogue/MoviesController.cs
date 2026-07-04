using StreamVault.Data.Catalogue;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Catalogue;

namespace StreamVault.Controllers.Catalogue;

public class MoviesController(ICatalogueRepository repository)
    : ContentEntryController<Movie, MovieFormViewModel>(repository);
