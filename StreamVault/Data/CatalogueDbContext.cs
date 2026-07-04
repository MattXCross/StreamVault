using Microsoft.EntityFrameworkCore;
using StreamVault.Models;

namespace StreamVault.Data;

public class CatalogueDbContext(DbContextOptions<CatalogueDbContext> options) : DbContext(options)
{
    public DbSet<ContentItem> Contents => Set<ContentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentItem>()
            .ToTable("Contents")
            .HasDiscriminator<string>("ContentType")
            .HasValue<Movie>("Movie")
            .HasValue<Series>("Series")
            .HasValue<Audiobook>("Audiobook")
            .HasValue<Album>("Album");

        modelBuilder.Entity<ContentItem>()
            .Property(c => c.Title)
            .HasMaxLength(200);

        // Movie and Audiobook durations mean the same thing, so share one column
        // rather than letting EF create a uniquified column per subclass.
        modelBuilder.Entity<Movie>()
            .Property(m => m.DurationMinutes)
            .HasColumnName("DurationMinutes");
        modelBuilder.Entity<Audiobook>()
            .Property(a => a.DurationMinutes)
            .HasColumnName("DurationMinutes");
    }
}
