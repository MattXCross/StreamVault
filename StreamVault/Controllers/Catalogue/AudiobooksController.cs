using StreamVault.Data.Catalogue;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Catalogue;

namespace StreamVault.Controllers.Catalogue;

public class AudiobooksController(ICatalogueRepository repository)
    : ContentEntryController<Audiobook, AudiobookFormViewModel>(repository);
