using Microsoft.AspNetCore.Mvc;
using StreamVault.Data;
using StreamVault.ViewModels;

namespace StreamVault.Controllers;

public class CatalogueController(ICatalogueRepository repository, IAnalyticsService analytics) : Controller
{
    public async Task<IActionResult> Index(string? type, string? search, int page = 1, int? expand = null, int days = 30)
    {
        var results = await repository.SearchAsync(new CatalogueQuery(type, search, page));

        ContentAnalyticsViewModel? panel = null;
        if (expand is int id && results.Items.Any(i => i.Id == id))
        {
            var window = ContentAnalyticsViewModel.WindowOptions.Contains(days) ? days : 30;
            panel = new ContentAnalyticsViewModel
            {
                ContentId = id,
                WindowDays = window,
                Stats = await analytics.GetStatsAsync(id),
                PlaysOverTime = await analytics.GetPlaysOverTimeAsync(id, window),
                Countries = await analytics.GetCountryBreakdownAsync(id, window)
            };
        }

        var model = new CatalogueListViewModel
        {
            Results = results,
            TypeFilter = type,
            SearchTerm = search,
            ExpandedId = expand,
            AnalyticsPanel = panel
        };
        return View(model);
    }
}
