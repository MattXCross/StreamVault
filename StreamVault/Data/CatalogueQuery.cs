namespace StreamVault.Data;

public record CatalogueQuery(
    string? ContentType = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10);
