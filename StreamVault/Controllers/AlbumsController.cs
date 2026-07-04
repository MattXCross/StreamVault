using StreamVault.Data;
using StreamVault.Models;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class AlbumsController(ICatalogueRepository repository)
    : ContentEntryController<Album, AlbumFormViewModel>(repository);
