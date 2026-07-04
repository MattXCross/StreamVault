using StreamVault.Data.Catalogue;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Catalogue;

namespace StreamVault.Controllers.Catalogue;

public class SeriesController(ICatalogueRepository repository)
    : ContentEntryController<Series, SeriesFormViewModel>(repository);
