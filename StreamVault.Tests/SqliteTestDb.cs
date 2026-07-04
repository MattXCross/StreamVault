using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StreamVault.Data;

namespace StreamVault.Tests;

public sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection Connection;

    public CatalogueDbContext Context { get; }

    public SqliteTestDb()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<CatalogueDbContext>()
            .UseSqlite(Connection)
            .Options;

        Context = new CatalogueDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
