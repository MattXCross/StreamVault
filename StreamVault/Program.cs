using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StreamVault.Configuration;
using StreamVault.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<AnalyticsOptions>(
    builder.Configuration.GetSection(AnalyticsOptions.SectionName));

builder.Services.AddDbContext<CatalogueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Catalogue")));
builder.Services.AddScoped<ICatalogueRepository, EfCatalogueRepository>();
builder.Services.AddScoped<IAnalyticsService, EfAnalyticsService>();

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