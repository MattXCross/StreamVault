using Microsoft.EntityFrameworkCore;
using StreamVault.Models;

namespace StreamVault.Data;

public class EfCatalogueRepository(CatalogueDbContext db) : ICatalogueRepository
{
    public async Task<IReadOnlyList<ContentItem>> SearchAsync(CatalogueQuery query)
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

        return await items.OrderBy(c => c.Title).ToListAsync();
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
