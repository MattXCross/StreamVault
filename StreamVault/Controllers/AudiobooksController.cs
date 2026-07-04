using StreamVault.Data;
using StreamVault.Models;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class AudiobooksController(ICatalogueRepository repository)
    : ContentEntryController<Audiobook, AudiobookFormViewModel>(repository);
