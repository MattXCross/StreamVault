using Microsoft.AspNetCore.Mvc;
using StreamVault.Data.Catalogue;
using StreamVault.Models.Catalogue;
using StreamVault.ViewModels.Catalogue;

namespace StreamVault.Controllers.Catalogue;

public abstract class ContentEntryController<TContent, TViewModel>(ICatalogueRepository repository) : Controller
    where TContent : ContentItem, new()
    where TViewModel : ContentFormViewModel<TContent>, new()
{
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var entity = new TContent();
        form.ApplyTo(entity);
        await repository.AddAsync(entity);

        TempData["Message"] = $"'{entity.Title}' was added to the catalogue.";
        return RedirectToAction("Index", "Catalogue");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await repository.FindAsync<TContent>(id);
        if (entity is null)
        {
            return NotFound();
        }

        var form = new TViewModel();
        form.LoadFrom(entity);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var entity = await repository.FindAsync<TContent>(id);
        if (entity is null)
        {
            return NotFound();
        }

        form.ApplyTo(entity);
        await repository.UpdateAsync(entity);

        TempData["Message"] = $"'{entity.Title}' was updated.";
        return RedirectToAction("Index", "Catalogue");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (deleted)
        {
            TempData["Message"] = "The item was removed from the catalogue.";
        }

        return RedirectToAction("Index", "Catalogue");
    }
}
