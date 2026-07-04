using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StreamVault.Configuration;
using StreamVault.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<AnalyticsOptions>(
    builder.Configuration.GetSection(AnalyticsOptions.SectionName));

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var usingSqlite = string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<CatalogueDbContext>(options =>
{
    if (usingSqlite)
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Catalogue") ?? "Data Source=streamvault.db");
    }
    else
    {
        throw new NotSupportedException($"Unsupported database provider '{databaseProvider}'. Only 'Sqlite' is implemented.");
    }
});
builder.Services.AddScoped<ICatalogueRepository, EfCatalogueRepository>();
builder.Services.AddScoped<IAnalyticsService, EfAnalyticsService>();

if (usingSqlite)
{
    builder.Services.AddHostedService<PlayEventPurgeService>();
}

var app = builder.Build();

app.Services.EnsureSeeded();

var analyticsOptions = app.Services.GetRequiredService<IOptions<AnalyticsOptions>>().Value;
if (analyticsOptions.EnableSimulator)
{
    app.Services.EnsureSimulated(analyticsOptions.PlayEventRetentionDays);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Catalogue}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();