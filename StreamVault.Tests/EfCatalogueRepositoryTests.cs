using StreamVault.Data.Catalogue;
using StreamVault.Models.Analytics;
using StreamVault.Models.Catalogue;

namespace StreamVault.Tests;

public class EfCatalogueRepositoryTests : IDisposable
{
    private readonly SqliteTestDb TestDb = new();
    private readonly EfCatalogueRepository Repository;

    public EfCatalogueRepositoryTests()
    {
        Repository = new EfCatalogueRepository(TestDb.Context);
    }

    public void Dispose() => TestDb.Dispose();

    private static Movie TestMovie(string title) => new()
    {
        Title = title,
        ReleaseDate = new DateOnly(2020, 1, 1),
        AgeRating = AgeRating.Universal,
        Genre = "Drama",
        Director = "Someone",
        DurationMinutes = 100
    };

    private void Seed(params ContentItem[] items)
    {
        TestDb.Context.Contents.AddRange(items);
        TestDb.Context.SaveChanges();
    }

    [Fact]
    public async Task SearchClampsToLastPage()
    {
        Seed(Enumerable.Range(1, 15).Select(i => (ContentItem)TestMovie($"Movie {i:D2}")).ToArray());

        var result = await Repository.SearchAsync(new CatalogueQuery(Page: 99));

        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(15, result.TotalCount);
    }

    [Fact]
    public async Task SearchHandlesEmptyCatalogue()
    {
        var result = await Repository.SearchAsync(new CatalogueQuery());

        Assert.Empty(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNext);
    }

    [Fact]
    public async Task SearchFiltersByContentType()
    {
        Seed(TestMovie("Apple"), new Series { Title = "Banana", Seasons = 1, TotalEpisodes = 8 });

        var result = await Repository.SearchAsync(new CatalogueQuery(ContentType: "Series"));

        var item = Assert.Single(result.Items);
        Assert.IsType<Series>(item);
    }

    [Fact]
    public async Task SearchMatchesSubstringsIgnoringCase()
    {
        Seed(TestMovie("The Silent Harbour"), TestMovie("Comet Season"));

        var result = await Repository.SearchAsync(new CatalogueQuery(SearchTerm: "SILENT"));

        Assert.Equal("The Silent Harbour", Assert.Single(result.Items).Title);
    }

    [Fact]
    public async Task DeleteCascadesAnalyticsData()
    {
        var movie = TestMovie("Apple");
        Seed(movie);
        TestDb.Context.ContentStats.Add(new ContentStats { ContentItemId = movie.Id, TotalPlays = 5 });
        TestDb.Context.PlayEvents.Add(new PlayEvent { ContentItemId = movie.Id, PlayedAt = DateTime.UtcNow, Country = "GB" });
        TestDb.Context.SaveChanges();

        Assert.True(await Repository.DeleteAsync(movie.Id));

        Assert.Empty(TestDb.Context.Contents.ToList());
        Assert.Empty(TestDb.Context.ContentStats.ToList());
        Assert.Empty(TestDb.Context.PlayEvents.ToList());
    }
}
