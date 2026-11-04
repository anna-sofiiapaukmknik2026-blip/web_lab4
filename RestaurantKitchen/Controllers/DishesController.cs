using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Models;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantKitchen.Controllers
{
    /// <summary>
    /// Перегляд і видалення страв меню.
    /// Створення та редагування виконує DishEditorController (дані зберігаються в сесії).
    /// </summary>
   
    [Authorize]
    public class DishesController : Controller
    {
        private readonly KitchenContext _context;

        public DishesController(KitchenContext context)
        {
            _context = context;
        }

        // GET: Dishes?search=борщ&category=Soup&onlyAvailable=true
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, DishCategory? category, bool onlyAvailable = false)
        {
            var query = _context.Dishes
                .Include(d => d.DishIngredients)
                    .ThenInclude(di => di.Ingredient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.Name.Contains(search));

            if (category.HasValue)
                query = query.Where(d => d.Category == category.Value);

            if (onlyAvailable)
                query = query.Where(d => d.IsAvailable);

            ViewData["Search"] = search;
            ViewData["Category"] = category;
            ViewData["OnlyAvailable"] = onlyAvailable;

            var dishes = await query
                .OrderBy(d => d.Category).ThenBy(d => d.Name)
                .ToListAsync();
            return View(dishes);
        }

        // GET: Dishes/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes
                .Include(d => d.DishIngredients)
                    .ThenInclude(di => di.Ingredient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null) return NotFound();
            return View(dish);
        }

        // GET: Dishes/Create → редактор із сесією
        public IActionResult Create() => RedirectToAction("Create", "DishEditor");

        // GET: Dishes/Edit/5 → редактор із сесією
        public IActionResult Edit(int? id) =>
            id == null ? NotFound() : RedirectToAction("Edit", "DishEditor", new { id });

        // GET: Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes
                .Include(d => d.DishIngredients)
                    .ThenInclude(di => di.Ingredient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null) return NotFound();
            return View(dish);
        }

        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                // Записи складу (DishIngredients) видаляються каскадно
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Страву «{dish.Name}» видалено.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}