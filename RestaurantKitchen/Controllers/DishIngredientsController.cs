using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Helpers;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>
    /// Керування складом страви (записи проміжної таблиці DishIngredients).
    /// Запис ідентифікується парою ключів (dishId, ingredientId).
    /// </summary>
    public class DishIngredientsController : Controller
    {
        private readonly KitchenContext _context;

        public DishIngredientsController(KitchenContext context)
        {
            _context = context;
        }

        // GET: DishIngredients — увесь склад усіх страв
        public async Task<IActionResult> Index()
        {
            var items = await _context.DishIngredients
                .Include(di => di.Dish)
                .Include(di => di.Ingredient)
                .OrderBy(di => di.Dish!.Name).ThenBy(di => di.Ingredient!.Name)
                .ToListAsync();
            return View(items);
        }

        // GET: DishIngredients/Create?dishId=1
        public async Task<IActionResult> Create(int dishId)
        {
            var dish = await _context.Dishes.FindAsync(dishId);
            if (dish == null) return NotFound();

            if (!await LoadIngredientListAsync(dishId))
            {
                TempData["Success"] = "До страви вже додано всі наявні інгредієнти.";
                return RedirectToAction("Details", "Dishes", new { id = dishId });
            }

            ViewData["DishName"] = dish.Name;
            return View(new DishIngredient { DishId = dishId });
        }

        // POST: DishIngredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DishId,IngredientId,Quantity,Note")] DishIngredient item)
        {
            var dish = await _context.Dishes.FindAsync(item.DishId);
            if (dish == null) return NotFound();

            if (!await _context.Ingredients.AnyAsync(i => i.Id == item.IngredientId))
                ModelState.AddModelError(nameof(DishIngredient.IngredientId), "Оберіть інгредієнт");

            if (await _context.DishIngredients.AnyAsync(di => di.DishId == item.DishId && di.IngredientId == item.IngredientId))
                ModelState.AddModelError(nameof(DishIngredient.IngredientId), "Цей інгредієнт уже є у складі страви");

            if (!ModelState.IsValid)
            {
                await LoadIngredientListAsync(item.DishId, item.IngredientId);
                ViewData["DishName"] = dish.Name;
                return View(item);
            }

            _context.DishIngredients.Add(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Інгредієнт додано до складу страви.";
            return RedirectToAction("Details", "Dishes", new { id = item.DishId });
        }

        // GET: DishIngredients/Edit?dishId=1&ingredientId=2
        public async Task<IActionResult> Edit(int dishId, int ingredientId)
        {
            var item = await FindAsync(dishId, ingredientId);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: DishIngredients/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int dishId, int ingredientId, [Bind("Quantity,Note")] DishIngredient input)
        {
            var item = await FindAsync(dishId, ingredientId);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
            {
                input.DishId = dishId;
                input.IngredientId = ingredientId;
                input.Dish = item.Dish;
                input.Ingredient = item.Ingredient;
                return View(input);
            }

            item.Quantity = input.Quantity;
            item.Note = input.Note;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Кількість «{item.Ingredient!.Name}» оновлено.";
            return RedirectToAction("Details", "Dishes", new { id = dishId });
        }

        // GET: DishIngredients/Delete?dishId=1&ingredientId=2
        public async Task<IActionResult> Delete(int dishId, int ingredientId)
        {
            var item = await FindAsync(dishId, ingredientId);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: DishIngredients/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int dishId, int ingredientId)
        {
            var item = await _context.DishIngredients.FindAsync(dishId, ingredientId);
            if (item != null)
            {
                _context.DishIngredients.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Інгредієнт прибрано зі складу страви.";
            }
            return RedirectToAction("Details", "Dishes", new { id = dishId });
        }

        private Task<DishIngredient?> FindAsync(int dishId, int ingredientId) =>
            _context.DishIngredients
                .Include(di => di.Dish)
                .Include(di => di.Ingredient)
                .FirstOrDefaultAsync(di => di.DishId == dishId && di.IngredientId == ingredientId);

        /// <summary>
        /// Заповнює список інгредієнтів, яких ще немає у складі страви.
        /// Повертає false, якщо додавати нічого.
        /// </summary>
        private async Task<bool> LoadIngredientListAsync(int dishId, int? selectedId = null)
        {
            var usedIds = await _context.DishIngredients
                .Where(di => di.DishId == dishId)
                .Select(di => di.IngredientId)
                .ToListAsync();

            var available = await _context.Ingredients
                .Where(i => !usedIds.Contains(i.Id))
                .OrderBy(i => i.Name)
                .ToListAsync();

            ViewData["IngredientId"] = new SelectList(
                available.Select(i => new { i.Id, Title = $"{i.Name} ({i.Unit.GetDisplayName()}, {i.PricePerUnit:0.00} грн/{i.Unit.GetDisplayName()})" }),
                "Id", "Title", selectedId);

            return available.Count > 0;
        }
    }
}