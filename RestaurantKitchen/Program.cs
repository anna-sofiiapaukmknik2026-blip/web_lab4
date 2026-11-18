using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Models;
using RestaurantKitchen.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Реєстрація контексту БД (SQL Server LocalDB)
builder.Services.AddDbContext<KitchenContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KitchenDb")));

// ---- Identity: користувачі та ролі, дані зберігаються в БД проєкту ----
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Вимоги до пароля
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;          // один email — один акаунт
    options.SignIn.RequireConfirmedAccount = false;  // без підтвердження пошти
})
.AddEntityFrameworkStores<KitchenContext>()           // зберігати в нашій БД
.AddDefaultTokenProviders()
.AddErrorDescriber<UkrainianIdentityErrorDescriber>();

// Налаштування cookie автентифікації
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";               // куди перенаправляти неавторизованих
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; // куди — якщо немає потрібної ролі
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ---- Сесії (лабораторна 2) ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".RestaurantKitchen.Session";
    options.Cookie.HttpOnly = true;
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

// Міграції + ролі та адміністратор
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KitchenContext>();
    db.Database.Migrate();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // хто цей користувач (читає cookie входу)
app.UseAuthorization();    // чи має він доступ (перевіряє [Authorize])

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");   // адреса, до якої підключається браузер

app.Run();