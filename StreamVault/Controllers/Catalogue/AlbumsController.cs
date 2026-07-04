using StreamVault.Data.Catalogue;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Catalogue;

namespace StreamVault.Controllers.Catalogue;

public class AlbumsController(ICatalogueRepository repository)
    : ContentEntryController<Album, AlbumFormViewModel>(repository);
