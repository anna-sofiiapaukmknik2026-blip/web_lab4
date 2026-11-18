using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RestaurantKitchen.Data
{
    public class KitchenContext : IdentityDbContext<ApplicationUser>
    {
        public KitchenContext(DbContextOptions<KitchenContext> options) : base(options) { }

        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<DishIngredient> DishIngredients => Set<DishIngredient>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Dish ----
            modelBuilder.Entity<Dish>(e =>
            {
                e.ToTable("Dishes");
                e.HasIndex(d => d.Name).IsUnique();
            });

            // ---- Ingredient ----
            modelBuilder.Entity<Ingredient>(e =>
            {
                e.ToTable("Ingredients");
                e.HasIndex(i => i.Name).IsUnique();
            });

            // ---- DishIngredient: зв'язок "багато-до-багатьох" ----
            modelBuilder.Entity<DishIngredient>(e =>
            {
                e.ToTable("DishIngredients");

                // Складений первинний ключ: один інгредієнт входить у страву лише один раз
                e.HasKey(di => new { di.DishId, di.IngredientId });

                // Dish 1 --- N DishIngredient
                e.HasOne(di => di.Dish)
                 .WithMany(d => d.DishIngredients)
                 .HasForeignKey(di => di.DishId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Ingredient 1 --- N DishIngredient
                e.HasOne(di => di.Ingredient)
                 .WithMany(i => i.DishIngredients)
                 .HasForeignKey(di => di.IngredientId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            Seed(modelBuilder);
        }

        private static void Seed(ModelBuilder mb)
        {
            mb.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Буряк", Unit = MeasureUnit.Kilogram, PricePerUnit = 18m, StockQuantity = 25m, CaloriesPer100g = 43, Supplier = "ФГ «Зелений лан»" },
                new Ingredient { Id = 2, Name = "Картопля", Unit = MeasureUnit.Kilogram, PricePerUnit = 15m, StockQuantity = 60m, CaloriesPer100g = 77, Supplier = "ФГ «Зелений лан»" },
                new Ingredient { Id = 3, Name = "Капуста білокачанна", Unit = MeasureUnit.Kilogram, PricePerUnit = 14m, StockQuantity = 20m, CaloriesPer100g = 27, Supplier = "ФГ «Зелений лан»" },
                new Ingredient { Id = 4, Name = "Свинина", Unit = MeasureUnit.Kilogram, PricePerUnit = 220m, StockQuantity = 15m, CaloriesPer100g = 242, Supplier = "М'ясний двір" },
                new Ingredient { Id = 5, Name = "Куряче філе", Unit = MeasureUnit.Kilogram, PricePerUnit = 190m, StockQuantity = 12m, CaloriesPer100g = 113, Supplier = "М'ясний двір" },
                new Ingredient { Id = 6, Name = "Борошно пшеничне", Unit = MeasureUnit.Kilogram, PricePerUnit = 25m, StockQuantity = 40m, CaloriesPer100g = 364, Supplier = "Млин Галичини", IsAllergen = true },
                new Ingredient { Id = 7, Name = "Яйце куряче", Unit = MeasureUnit.Piece, PricePerUnit = 5.5m, StockQuantity = 180m, CaloriesPer100g = 157, Supplier = "Птахофабрика «Лан»", IsAllergen = true },
                new Ingredient { Id = 8, Name = "Сметана 20%", Unit = MeasureUnit.Kilogram, PricePerUnit = 95m, StockQuantity = 8m, CaloriesPer100g = 206, Supplier = "Молочна ферма", IsAllergen = true },
                new Ingredient { Id = 9, Name = "Масло вершкове", Unit = MeasureUnit.Kilogram, PricePerUnit = 380m, StockQuantity = 6m, CaloriesPer100g = 717, Supplier = "Молочна ферма", IsAllergen = true },
                new Ingredient { Id = 10, Name = "Цибуля ріпчаста", Unit = MeasureUnit.Kilogram, PricePerUnit = 16m, StockQuantity = 25m, CaloriesPer100g = 40, Supplier = "ФГ «Зелений лан»" },
                new Ingredient { Id = 11, Name = "Сир кисломолочний", Unit = MeasureUnit.Kilogram, PricePerUnit = 140m, StockQuantity = 10m, CaloriesPer100g = 159, Supplier = "Молочна ферма", IsAllergen = true },
                new Ingredient { Id = 12, Name = "Сухофрукти", Unit = MeasureUnit.Kilogram, PricePerUnit = 160m, StockQuantity = 5m, CaloriesPer100g = 249, Supplier = "Карпатські сади" },
                new Ingredient { Id = 13, Name = "Цукор", Unit = MeasureUnit.Kilogram, PricePerUnit = 30m, StockQuantity = 25m, CaloriesPer100g = 387 },
                new Ingredient { Id = 14, Name = "Морква", Unit = MeasureUnit.Kilogram, PricePerUnit = 17m, StockQuantity = 20m, CaloriesPer100g = 41, Supplier = "ФГ «Зелений лан»" },
                new Ingredient { Id = 15, Name = "Сухарі панірувальні", Unit = MeasureUnit.Kilogram, PricePerUnit = 60m, StockQuantity = 4m, CaloriesPer100g = 395, IsAllergen = true }
            );

            mb.Entity<Dish>().HasData(
                new Dish { Id = 1, Name = "Борщ український", Category = DishCategory.Soup, Price = 145m, CookingTimeMinutes = 90, PortionWeightGrams = 350, IsAvailable = true, CreatedAt = new DateTime(2026, 9, 1), Description = "Класичний червоний борщ зі свининою, подається зі сметаною." },
                new Dish { Id = 2, Name = "Вареники з картоплею", Category = DishCategory.MainCourse, Price = 120m, CookingTimeMinutes = 40, PortionWeightGrams = 250, IsAvailable = true, CreatedAt = new DateTime(2026, 9, 1), Description = "Вареники ручної ліпки з картоплею та цибулевою засмажкою." },
                new Dish { Id = 3, Name = "Котлета по-київськи", Category = DishCategory.MainCourse, Price = 210m, CookingTimeMinutes = 35, PortionWeightGrams = 230, IsAvailable = true, CreatedAt = new DateTime(2026, 9, 5), Description = "Куряче філе з вершковим маслом у паніровці." },
                new Dish { Id = 4, Name = "Сирники зі сметаною", Category = DishCategory.Dessert, Price = 115m, CookingTimeMinutes = 20, PortionWeightGrams = 200, IsAvailable = true, CreatedAt = new DateTime(2026, 9, 10), Description = "Сирники з кисломолочного сиру, подаються зі сметаною." },
                new Dish { Id = 5, Name = "Узвар", Category = DishCategory.Drink, Price = 45m, CookingTimeMinutes = 30, PortionWeightGrams = 300, IsAvailable = false, CreatedAt = new DateTime(2026, 9, 12), Description = "Сезонний напій із сухофруктів." },
                new Dish { Id = 6, Name = "Деруни зі сметаною", Category = DishCategory.Appetizer, Price = 105m, CookingTimeMinutes = 25, PortionWeightGrams = 220, IsAvailable = true, CreatedAt = new DateTime(2026, 9, 15), Description = "Картопляні деруни, подаються гарячими зі сметаною." }
            );

            mb.Entity<DishIngredient>().HasData(
                // Борщ
                new DishIngredient { DishId = 1, IngredientId = 1, Quantity = 0.08m },
                new DishIngredient { DishId = 1, IngredientId = 3, Quantity = 0.06m },
                new DishIngredient { DishId = 1, IngredientId = 2, Quantity = 0.06m },
                new DishIngredient { DishId = 1, IngredientId = 4, Quantity = 0.08m },
                new DishIngredient { DishId = 1, IngredientId = 14, Quantity = 0.03m },
                new DishIngredient { DishId = 1, IngredientId = 10, Quantity = 0.03m },
                new DishIngredient { DishId = 1, IngredientId = 8, Quantity = 0.03m, Note = "Для подачі" },
                // Вареники
                new DishIngredient { DishId = 2, IngredientId = 6, Quantity = 0.1m, Note = "Тісто" },
                new DishIngredient { DishId = 2, IngredientId = 7, Quantity = 0.5m, Note = "Тісто" },
                new DishIngredient { DishId = 2, IngredientId = 2, Quantity = 0.15m, Note = "Начинка" },
                new DishIngredient { DishId = 2, IngredientId = 10, Quantity = 0.03m, Note = "Засмажка" },
                new DishIngredient { DishId = 2, IngredientId = 9, Quantity = 0.01m },
                // Котлета по-київськи
                new DishIngredient { DishId = 3, IngredientId = 5, Quantity = 0.15m },
                new DishIngredient { DishId = 3, IngredientId = 9, Quantity = 0.03m, Note = "Начинка" },
                new DishIngredient { DishId = 3, IngredientId = 7, Quantity = 1m, Note = "Панірування" },
                new DishIngredient { DishId = 3, IngredientId = 15, Quantity = 0.03m },
                new DishIngredient { DishId = 3, IngredientId = 6, Quantity = 0.01m },
                // Сирники
                new DishIngredient { DishId = 4, IngredientId = 11, Quantity = 0.15m },
                new DishIngredient { DishId = 4, IngredientId = 7, Quantity = 1m },
                new DishIngredient { DishId = 4, IngredientId = 6, Quantity = 0.03m },
                new DishIngredient { DishId = 4, IngredientId = 13, Quantity = 0.015m },
                new DishIngredient { DishId = 4, IngredientId = 8, Quantity = 0.04m, Note = "Для подачі" },
                // Узвар
                new DishIngredient { DishId = 5, IngredientId = 12, Quantity = 0.04m },
                new DishIngredient { DishId = 5, IngredientId = 13, Quantity = 0.02m },
                // Деруни
                new DishIngredient { DishId = 6, IngredientId = 2, Quantity = 0.2m },
                new DishIngredient { DishId = 6, IngredientId = 10, Quantity = 0.03m },
                new DishIngredient { DishId = 6, IngredientId = 7, Quantity = 1m },
                new DishIngredient { DishId = 6, IngredientId = 6, Quantity = 0.02m },
                new DishIngredient { DishId = 6, IngredientId = 8, Quantity = 0.04m, Note = "Для подачі" }
            );
        }
    }
}