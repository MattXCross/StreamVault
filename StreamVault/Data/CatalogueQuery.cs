namespace StreamVault.Data;

/// <summary>
/// Filter criteria for the catalogue list. ContentType matches the
/// discriminator values configured in <see cref="CatalogueDbContext"/>.
/// </summary>
public record CatalogueQuery(string? ContentType = null, string? SearchTerm = null);
