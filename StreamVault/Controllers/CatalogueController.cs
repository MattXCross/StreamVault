using Microsoft.AspNetCore.Mvc;
using StreamVault.Data;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class CatalogueController(ICatalogueRepository repository) : Controller
{
    public async Task<IActionResult> Index(string? type, string? search, int page = 1)
    {
        var results = await repository.SearchAsync(new CatalogueQuery(type, search, page));

        var model = new CatalogueListViewModel
        {
            Results = results,
            TypeFilter = type,
            SearchTerm = search
        };
        return View(model);
    }
}
