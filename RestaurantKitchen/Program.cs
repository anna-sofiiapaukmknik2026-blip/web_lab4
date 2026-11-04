using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Реєстрація контексту БД (SQL Server LocalDB)
builder.Services.AddDbContext<KitchenContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KitchenDb")));

// ---- Сесії ----
// Сховище для даних сесії (у пам'яті сервера)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);      // сесія живе 30 хв без активності
    options.Cookie.Name = ".RestaurantKitchen.Session";
    options.Cookie.HttpOnly = true;                       // cookie недоступна з JavaScript
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Десяткові числа вводимо через крапку (0.25)
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

// Застосування міграцій під час запуску
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KitchenContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // підключення сесій до конвеєра обробки запитів
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();