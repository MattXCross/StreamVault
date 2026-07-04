using Microsoft.AspNetCore.Mvc;
using StreamVault.Data;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class CatalogueController(ICatalogueRepository repository) : Controller
{
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var items = await repository.SearchAsync(new CatalogueQuery(type, search));

        var model = new CatalogueListViewModel
        {
            Items = items,
            TypeFilter = type,
            SearchTerm = search
        };
        return View(model);
    }
}
