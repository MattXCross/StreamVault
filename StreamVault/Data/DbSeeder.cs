using Microsoft.Extensions.Options;
using StreamVault.Configuration;
using StreamVault.Data.Analytics;
using StreamVault.Models.Catalogue;

namespace StreamVault.Data;

public static class DbSeeder
{
    public static void EnsureSeeded(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        db.Database.EnsureCreated();

        if (!db.Contents.Any())
        {
            db.Contents.AddRange(Movies());
            db.Contents.AddRange(SeriesList());
            db.Contents.AddRange(Audiobooks());
            db.Contents.AddRange(Albums());
            db.SaveChanges();
        }

        var options = scope.ServiceProvider.GetRequiredService<IOptions<AnalyticsOptions>>().Value;
        if (options.EnableSimulator && !db.ContentStats.Any())
        {
            var simulator = scope.ServiceProvider.GetRequiredService<AnalyticsSimulator>();

            foreach (var contentId in db.Contents.Select(c => c.Id).ToList())
            {
                var plays = simulator.SimulateHistory(contentId, options.PlayEventRetentionDays);
                db.PlayEvents.AddRange(plays);
                db.ContentStats.Add(simulator.SimulateStats(contentId, plays.Count));
            }

            db.SaveChanges();
        }
    }

    private static IEnumerable<Movie> Movies() =>
    [
        new()
        {
            Title = "The Silent Harbour", Description = "A retired detective is drawn back for one last case.",
            ReleaseDate = new DateOnly(2021, 3, 12), AgeRating = AgeRating.Fifteen, Genre = "Thriller",
            DurationMinutes = 128, Director = "Elena Marsh"
        },
        new()
        {
            Title = "Paper Planets", Description = "An animator discovers her drawings predict the future.",
            ReleaseDate = new DateOnly(2023, 7, 21), AgeRating = AgeRating.ParentalGuidance, Genre = "Fantasy",
            DurationMinutes = 104, Director = "Tomas Reyes"
        },
        new()
        {
            Title = "Iron Tide", Description = "A submarine crew races to stop a rogue AI fleet.",
            ReleaseDate = new DateOnly(2019, 11, 8), AgeRating = AgeRating.Twelve, Genre = "Action",
            DurationMinutes = 117, Director = "Kwame Osei"
        },
        new()
        {
            Title = "Beneath the Orchard", Description = "Two sisters inherit a farm and its buried past.",
            ReleaseDate = new DateOnly(2020, 9, 4), AgeRating = AgeRating.Twelve, Genre = "Drama",
            DurationMinutes = 111, Director = "Sofia Antonelli"
        },
        new()
        {
            Title = "Neon Getaway", Description = "A getaway driver takes one final job in Macau.",
            ReleaseDate = new DateOnly(2022, 2, 18), AgeRating = AgeRating.Fifteen, Genre = "Crime",
            DurationMinutes = 96, Director = "Elena Marsh"
        },
        new()
        {
            Title = "The Glass Meridian", Description = "An archaeologist decodes a map hidden in cathedral glass.",
            ReleaseDate = new DateOnly(2018, 6, 1), AgeRating = AgeRating.ParentalGuidance, Genre = "Adventure",
            DurationMinutes = 133, Director = "Henrik Dahl"
        },
        new()
        {
            Title = "Comet Season", Description = "A small town prepares for a once-a-century comet.",
            ReleaseDate = new DateOnly(2024, 4, 5), AgeRating = AgeRating.Universal, Genre = "Family",
            DurationMinutes = 92, Director = "Priya Raman"
        },
        new()
        {
            Title = "Static", Description = "A late-night radio host hears tomorrow's news.",
            ReleaseDate = new DateOnly(2017, 10, 27), AgeRating = AgeRating.Fifteen, Genre = "Horror",
            DurationMinutes = 101, Director = "Jordan Blake"
        },
        new()
        {
            Title = "The Long Thaw", Description = "Rivals must cross a melting ice field together.",
            ReleaseDate = new DateOnly(2021, 1, 15), AgeRating = AgeRating.Twelve, Genre = "Survival",
            DurationMinutes = 124, Director = "Anya Petrova"
        },
        new()
        {
            Title = "Counterfeit Sky", Description = "A forger is hired to fake a satellite feed.",
            ReleaseDate = new DateOnly(2023, 8, 11), AgeRating = AgeRating.Fifteen, Genre = "Thriller",
            DurationMinutes = 109, Director = "Kwame Osei"
        },
        new()
        {
            Title = "Waltz for Three", Description = "A chamber trio falls apart on their farewell tour.",
            ReleaseDate = new DateOnly(2016, 12, 9), AgeRating = AgeRating.ParentalGuidance, Genre = "Drama",
            DurationMinutes = 118, Director = "Margot Fournier"
        },
        new()
        {
            Title = "Zero Hour Bakery", Description = "A heist crew hides out by running a night bakery.",
            ReleaseDate = new DateOnly(2025, 5, 30), AgeRating = AgeRating.Twelve, Genre = "Comedy",
            DurationMinutes = 98, Director = "Tomas Reyes"
        }
    ];

    private static IEnumerable<Series> SeriesList() =>
    [
        new()
        {
            Title = "Northern Lights", Description = "A small Arctic research town hides a big secret.",
            ReleaseDate = new DateOnly(2019, 10, 4), AgeRating = AgeRating.Twelve, Genre = "Mystery",
            Seasons = 3, TotalEpisodes = 24
        },
        new()
        {
            Title = "Test Kitchen", Description = "Chefs compete to reinvent classic dishes.",
            ReleaseDate = new DateOnly(2022, 1, 15), AgeRating = AgeRating.Universal, Genre = "Reality",
            Seasons = 2, TotalEpisodes = 16
        },
        new()
        {
            Title = "Harbour Row", Description = "Three families share a dock and decades of grudges.",
            ReleaseDate = new DateOnly(2018, 3, 22), AgeRating = AgeRating.Twelve, Genre = "Drama",
            Seasons = 5, TotalEpisodes = 52
        },
        new()
        {
            Title = "Signal Lost", Description = "A missing streamer's chat tries to solve her disappearance.",
            ReleaseDate = new DateOnly(2023, 9, 1), AgeRating = AgeRating.Fifteen, Genre = "Thriller",
            Seasons = 1, TotalEpisodes = 8
        },
        new()
        {
            Title = "The Understudy", Description = "A West End understudy schemes her way to the lead.",
            ReleaseDate = new DateOnly(2021, 5, 14), AgeRating = AgeRating.Fifteen, Genre = "Dark Comedy",
            Seasons = 2, TotalEpisodes = 12
        },
        new()
        {
            Title = "Wild Kitchen Garden", Description = "Foragers turn hedgerow finds into restaurant plates.",
            ReleaseDate = new DateOnly(2020, 4, 10), AgeRating = AgeRating.Universal, Genre = "Documentary",
            Seasons = 4, TotalEpisodes = 32
        },
        new()
        {
            Title = "Redline", Description = "Street mechanics build cars for an illegal endurance race.",
            ReleaseDate = new DateOnly(2024, 2, 23), AgeRating = AgeRating.Twelve, Genre = "Action",
            Seasons = 1, TotalEpisodes = 10
        },
        new()
        {
            Title = "Paper Trail", Description = "A junior auditor unravels a charity's missing millions.",
            ReleaseDate = new DateOnly(2017, 11, 3), AgeRating = AgeRating.Twelve, Genre = "Crime",
            Seasons = 3, TotalEpisodes = 18
        }
    ];

    private static IEnumerable<Audiobook> Audiobooks() =>
    [
        new()
        {
            Title = "The Cartographer's Daughter", Description = "A sweeping historical epic across three continents.",
            ReleaseDate = new DateOnly(2020, 5, 8), AgeRating = AgeRating.Twelve, Genre = "Historical Fiction",
            Author = "Priya Nair", Narrator = "James Holloway", DurationMinutes = 812
        },
        new()
        {
            Title = "Deep Work Habits", Description = "Practical strategies for focused productivity.",
            ReleaseDate = new DateOnly(2024, 2, 1), AgeRating = AgeRating.Universal, Genre = "Self-Help",
            Author = "Marcus Chen", Narrator = "Sofia Lindqvist", DurationMinutes = 465
        },
        new()
        {
            Title = "Salt and Starlight", Description = "A lighthouse keeper's memoir of forty winters.",
            ReleaseDate = new DateOnly(2019, 8, 16), AgeRating = AgeRating.Universal, Genre = "Memoir",
            Author = "Eileen Brody", Narrator = "Eileen Brody", DurationMinutes = 534
        },
        new()
        {
            Title = "The Hollow Crown Affair", Description = "A conman targets a crumbling aristocratic estate.",
            ReleaseDate = new DateOnly(2022, 10, 7), AgeRating = AgeRating.Twelve, Genre = "Mystery",
            Author = "Rex Callahan", Narrator = "Imogen Hart", DurationMinutes = 701
        },
        new()
        {
            Title = "Fermentation Nation", Description = "The science and folklore of pickled everything.",
            ReleaseDate = new DateOnly(2023, 3, 24), AgeRating = AgeRating.Universal, Genre = "Food",
            Author = "Dana Okafor", Narrator = "Miles Turner", DurationMinutes = 388
        },
        new()
        {
            Title = "Orbit of Glass", Description = "Colonists on a glass-domed moon face a slow leak.",
            ReleaseDate = new DateOnly(2021, 12, 3), AgeRating = AgeRating.Fifteen, Genre = "Science Fiction",
            Author = "Theo Lindgren", Narrator = "Yuki Tanaka", DurationMinutes = 923
        }
    ];

    private static IEnumerable<Album> Albums() =>
    [
        new()
        {
            Title = "Midnight Frequencies", Description = "Synth-heavy debut from the Berlin duo.",
            ReleaseDate = new DateOnly(2022, 11, 18), AgeRating = AgeRating.ParentalGuidance, Genre = "Electronic",
            Artist = "Neon Atlas", TrackCount = 11, RecordLabel = "Nocturne Records"
        },
        new()
        {
            Title = "Harvest Moon Sessions", Description = "Live acoustic recordings from the harvest tour.",
            ReleaseDate = new DateOnly(2018, 9, 28), AgeRating = AgeRating.Universal, Genre = "Folk",
            Artist = "Willa Hart", TrackCount = 9, RecordLabel = "Homestead Music"
        },
        new()
        {
            Title = "Concrete Bloom", Description = "Garage rock recorded in an actual garage.",
            ReleaseDate = new DateOnly(2021, 6, 11), AgeRating = AgeRating.Twelve, Genre = "Rock",
            Artist = "The Fire Escapes", TrackCount = 10, RecordLabel = "Brickyard Sound"
        },
        new()
        {
            Title = "Tides & Tempo", Description = "Modern jazz suite inspired by shipping forecasts.",
            ReleaseDate = new DateOnly(2020, 2, 14), AgeRating = AgeRating.Universal, Genre = "Jazz",
            Artist = "Mariana Cole Quartet", TrackCount = 7, RecordLabel = "Blue Harbour"
        },
        new()
        {
            Title = "Static Bloom", Description = "Glitch-pop with samples from dead radio bands.",
            ReleaseDate = new DateOnly(2024, 8, 30), AgeRating = AgeRating.ParentalGuidance, Genre = "Pop",
            Artist = "Neon Atlas", TrackCount = 12, RecordLabel = "Nocturne Records"
        },
        new()
        {
            Title = "Milemarkers", Description = "Country ballads written across ten states.",
            ReleaseDate = new DateOnly(2019, 4, 26), AgeRating = AgeRating.Universal, Genre = "Country",
            Artist = "June Calloway", TrackCount = 11, RecordLabel = "Open Road Records"
        }
    ];
}
