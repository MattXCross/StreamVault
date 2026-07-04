using StreamVault.Models.Catalogue;

namespace StreamVault.Data.Catalogue;

public interface ICatalogueRepository
{
    Task<PagedResult<ContentItem>> SearchAsync(CatalogueQuery query);

    Task<T?> FindAsync<T>(int id) where T : ContentItem;

    Task AddAsync(ContentItem item);

    Task UpdateAsync(ContentItem item);
    
    Task<bool> DeleteAsync(int id);
}
