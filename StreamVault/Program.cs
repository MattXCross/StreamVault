using Microsoft.EntityFrameworkCore;
using StreamVault.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CatalogueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Catalogue")));
builder.Services.AddScoped<ICatalogueRepository, EfCatalogueRepository>();

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