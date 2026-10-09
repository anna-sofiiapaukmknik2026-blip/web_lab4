using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>CRUD-операції для страв меню.</summary>
    public class DishesController : Controller
    {
        private readonly KitchenContext _context;

        public DishesController(KitchenContext context)
        {
            _context = context;
        }

        // GET: Dishes?search=борщ&category=Soup&onlyAvailable=true
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

        // GET: Dishes/Create
        public IActionResult Create()
        {
            return View(new Dish { IsAvailable = true, CreatedAt = DateTime.Today });
        }

        // POST: Dishes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,Category,Price,CookingTimeMinutes,PortionWeightGrams,IsAvailable,CreatedAt")] Dish dish)
        {
            await ValidateUniqueNameAsync(dish);

            if (!ModelState.IsValid) return View(dish);

            _context.Add(dish);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Страву «{dish.Name}» додано. Тепер вкажіть її склад.";
            return RedirectToAction(nameof(Details), new { id = dish.Id });
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Description,Category,Price,CookingTimeMinutes,PortionWeightGrams,IsAvailable,CreatedAt")] Dish dish)
        {
            if (id != dish.Id) return NotFound();

            await ValidateUniqueNameAsync(dish);

            if (!ModelState.IsValid) return View(dish);

            try
            {
                _context.Update(dish);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Dishes.AnyAsync(d => d.Id == dish.Id)) return NotFound();
                throw;
            }

            TempData["Success"] = $"Зміни у страві «{dish.Name}» збережено.";
            return RedirectToAction(nameof(Details), new { id = dish.Id });
        }

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

        private async Task ValidateUniqueNameAsync(Dish dish)
        {
            var name = dish.Name?.Trim() ?? string.Empty;
            if (await _context.Dishes.AnyAsync(d => d.Name == name && d.Id != dish.Id))
                ModelState.AddModelError(nameof(Dish.Name), "Страва з такою назвою вже існує");
        }
    }
}