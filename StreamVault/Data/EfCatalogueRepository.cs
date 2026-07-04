using Microsoft.EntityFrameworkCore;
using StreamVault.Models;

namespace StreamVault.Data;

public class EfCatalogueRepository(CatalogueDbContext db) : ICatalogueRepository
{
    public async Task<PagedResult<ContentItem>> SearchAsync(CatalogueQuery query)
    {
        IQueryable<ContentItem> items = db.Contents;

        if (!string.IsNullOrWhiteSpace(query.ContentType))
        {
            items = items.Where(c => EF.Property<string>(c, "ContentType") == query.ContentType);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            items = items.Where(c => EF.Functions.Like(c.Title, $"%{query.SearchTerm}%"));
        }

        var totalCount = await items.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)query.PageSize));
        var page = Math.Clamp(query.Page, 1, totalPages);

        var pageItems = await items
            .OrderBy(c => c.Title)
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<ContentItem>(pageItems, totalCount, page, query.PageSize);
    }

    public async Task<T?> FindAsync<T>(int id) where T : ContentItem
    {
        return await db.Set<T>().SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(ContentItem item)
    {
        db.Contents.Add(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContentItem item)
    {
        db.Contents.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await db.Contents.SingleOrDefaultAsync(c => c.Id == id);
        if (item is null)
        {
            return false;
        }

        db.Contents.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}
