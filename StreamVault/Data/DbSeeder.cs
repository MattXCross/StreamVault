using StreamVault.Models;

namespace StreamVault.Data;

public static class DbSeeder
{
    public static void EnsureSeeded(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        db.Database.EnsureCreated();

        if (db.Contents.Any())
        {
            return;
        }

        db.Contents.AddRange(
            new Movie
            {
                Title = "The Silent Harbour",
                Description = "A retired detective is drawn back for one last case.",
                ReleaseDate = new DateOnly(2021, 3, 12),
                AgeRating = AgeRating.Fifteen,
                Genre = "Thriller",
                DurationMinutes = 128,
                Director = "Elena Marsh"
            },
            new Movie
            {
                Title = "Paper Planets",
                Description = "An animator discovers her drawings predict the future.",
                ReleaseDate = new DateOnly(2023, 7, 21),
                AgeRating = AgeRating.ParentalGuidance,
                Genre = "Fantasy",
                DurationMinutes = 104,
                Director = "Tomas Reyes"
            },
            new Series
            {
                Title = "Northern Lights",
                Description = "A small Arctic research town hides a big secret.",
                ReleaseDate = new DateOnly(2019, 10, 4),
                AgeRating = AgeRating.Twelve,
                Genre = "Mystery",
                Seasons = 3,
                TotalEpisodes = 24
            },
            new Series
            {
                Title = "Test Kitchen",
                Description = "Chefs compete to reinvent classic dishes.",
                ReleaseDate = new DateOnly(2022, 1, 15),
                AgeRating = AgeRating.Universal,
                Genre = "Reality",
                Seasons = 2,
                TotalEpisodes = 16
            },
            new Audiobook
            {
                Title = "The Cartographer's Daughter",
                Description = "A sweeping historical epic across three continents.",
                ReleaseDate = new DateOnly(2020, 5, 8),
                AgeRating = AgeRating.Twelve,
                Genre = "Historical Fiction",
                Author = "Priya Nair",
                Narrator = "James Holloway",
                DurationMinutes = 812
            },
            new Audiobook
            {
                Title = "Deep Work Habits",
                Description = "Practical strategies for focused productivity.",
                ReleaseDate = new DateOnly(2024, 2, 1),
                AgeRating = AgeRating.Universal,
                Genre = "Self-Help",
                Author = "Marcus Chen",
                Narrator = "Sofia Lindqvist",
                DurationMinutes = 465
            },
            new Album
            {
                Title = "Midnight Frequencies",
                Description = "Synth-heavy debut from the Berlin duo.",
                ReleaseDate = new DateOnly(2022, 11, 18),
                AgeRating = AgeRating.ParentalGuidance,
                Genre = "Electronic",
                Artist = "Neon Atlas",
                TrackCount = 11,
                RecordLabel = "Nocturne Records"
            },
            new Album
            {
                Title = "Harvest Moon Sessions",
                Description = "Live acoustic recordings from the harvest tour.",
                ReleaseDate = new DateOnly(2018, 9, 28),
                AgeRating = AgeRating.Universal,
                Genre = "Folk",
                Artist = "Willa Hart",
                TrackCount = 9,
                RecordLabel = "Homestead Music"
            }
        );

        db.SaveChanges();
    }
}
