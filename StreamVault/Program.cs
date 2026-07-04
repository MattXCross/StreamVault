using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSingleton<IAnalyticsSimulator, AnalyticsSimulator>();

if (usingSqlite)
{
    builder.Services.AddHostedService<PlayEventPurgeService>();
}

var analyticsOptions = builder.Configuration.GetSection(AnalyticsOptions.SectionName).Get<AnalyticsOptions>() ?? new AnalyticsOptions();
if (analyticsOptions.EnableSimulator)
{
    builder.Services.AddHostedService<AnalyticsSimulationService>();
}

var app = builder.Build();

app.Services.EnsureSeeded();

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