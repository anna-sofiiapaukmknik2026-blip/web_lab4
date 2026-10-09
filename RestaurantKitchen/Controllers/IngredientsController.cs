using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>CRUD-операції для інгредієнтів.</summary>
    public class IngredientsController : Controller
    {
        private readonly KitchenContext _context;

        public IngredientsController(KitchenContext context)
        {
            _context = context;
        }

        // GET: Ingredients?search=сир&onlyAllergens=true&sort=price
        public async Task<IActionResult> Index(string? search, bool onlyAllergens = false, string? sort = null)
        {
            var query = _context.Ingredients
                .Include(i => i.DishIngredients)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.Name.Contains(search) || (i.Supplier != null && i.Supplier.Contains(search)));

            if (onlyAllergens)
                query = query.Where(i => i.IsAllergen);

            query = sort switch
            {
                "price" => query.OrderByDescending(i => i.PricePerUnit),
                "stock" => query.OrderBy(i => i.StockQuantity),
                _ => query.OrderBy(i => i.Name)
            };

            ViewData["Search"] = search;
            ViewData["OnlyAllergens"] = onlyAllergens;
            ViewData["Sort"] = sort;

            return View(await query.ToListAsync());
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ingredient = await _context.Ingredients
                .Include(i => i.DishIngredients)
                    .ThenInclude(di => di.Dish)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            return View(new Ingredient { Unit = MeasureUnit.Kilogram });
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Unit,PricePerUnit,StockQuantity,CaloriesPer100g,Supplier,IsAllergen")] Ingredient ingredient)
        {
            await ValidateUniqueNameAsync(ingredient);

            if (!ModelState.IsValid) return View(ingredient);

            _context.Add(ingredient);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Інгредієнт «{ingredient.Name}» додано.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // POST: Ingredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Unit,PricePerUnit,StockQuantity,CaloriesPer100g,Supplier,IsAllergen")] Ingredient ingredient)
        {
            if (id != ingredient.Id) return NotFound();

            await ValidateUniqueNameAsync(ingredient);

            if (!ModelState.IsValid) return View(ingredient);

            try
            {
                _context.Update(ingredient);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Ingredients.AnyAsync(i => i.Id == ingredient.Id)) return NotFound();
                throw;
            }

            TempData["Success"] = $"Зміни в інгредієнті «{ingredient.Name}» збережено.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ingredients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ingredient = await _context.Ingredients
                .Include(i => i.DishIngredients)
                    .ThenInclude(di => di.Dish)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // POST: Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Інгредієнт «{ingredient.Name}» видалено.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateUniqueNameAsync(Ingredient ingredient)
        {
            var name = ingredient.Name?.Trim() ?? string.Empty;
            if (await _context.Ingredients.AnyAsync(i => i.Name == name && i.Id != ingredient.Id))
                ModelState.AddModelError(nameof(Ingredient.Name), "Інгредієнт з такою назвою вже існує");
        }
    }
}