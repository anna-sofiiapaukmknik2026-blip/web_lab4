using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    public class HomeController : Controller
    {
        private const decimal LowStockThreshold = 5m;
        private readonly KitchenContext _context;

        public HomeController(KitchenContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                DishCount = await _context.Dishes.CountAsync(),
                AvailableDishCount = await _context.Dishes.CountAsync(d => d.IsAvailable),
                IngredientCount = await _context.Ingredients.CountAsync(),
                AllergenCount = await _context.Ingredients.CountAsync(i => i.IsAllergen),
                LowStockIngredients = await _context.Ingredients
                    .Where(i => i.StockQuantity < LowStockThreshold)
                    .OrderBy(i => i.StockQuantity)
                    .ToListAsync(),
                LatestDishes = await _context.Dishes
                    .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id)
                    .Take(5)
                    .ToListAsync()
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}