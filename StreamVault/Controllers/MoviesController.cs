using StreamVault.Data;
using StreamVault.Models;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class MoviesController(ICatalogueRepository repository)
    : ContentEntryController<Movie, MovieFormViewModel>(repository);
