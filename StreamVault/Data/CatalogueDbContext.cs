using Microsoft.EntityFrameworkCore;
using StreamVault.Models;

namespace StreamVault.Data;

public class CatalogueDbContext(DbContextOptions<CatalogueDbContext> options) : DbContext(options)
{
    public DbSet<ContentItem> Contents => Set<ContentItem>();

    public DbSet<ContentStats> ContentStats => Set<ContentStats>();

    public DbSet<PlayEvent> PlayEvents => Set<PlayEvent>();

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

        modelBuilder.Entity<Movie>()
            .Property(m => m.DurationMinutes)
            .HasColumnName("DurationMinutes");
        modelBuilder.Entity<Audiobook>()
            .Property(a => a.DurationMinutes)
            .HasColumnName("DurationMinutes");

        modelBuilder.Entity<ContentStats>()
            .HasOne<ContentItem>()
            .WithOne()
            .HasForeignKey<ContentStats>(s => s.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayEvent>()
            .HasOne<ContentItem>()
            .WithMany()
            .HasForeignKey(e => e.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayEvent>()
            .HasIndex(e => new { e.ContentItemId, e.PlayedAt });
    }
}
