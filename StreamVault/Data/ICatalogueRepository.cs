using StreamVault.Models;

namespace StreamVault.Data;

public interface ICatalogueRepository
{
    Task<IReadOnlyList<ContentItem>> SearchAsync(CatalogueQuery query);

    Task<T?> FindAsync<T>(int id) where T : ContentItem;

    Task AddAsync(ContentItem item);

    Task UpdateAsync(ContentItem item);
    
    Task<bool> DeleteAsync(int id);
}
