using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Helpers;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>
    /// Створення та редагування страви через чернетку в сесії.
    /// Дані з БД копіюються в сесію, усі зміни (поля і склад) накопичуються в сесії
    /// та записуються в БД лише після натискання «Зберегти в базу». Після запису сесія очищається.
    /// </summary>
    public class DishEditorController : Controller
    {
        private readonly KitchenContext _context;

        public DishEditorController(KitchenContext context)
        {
            _context = context;
        }

        /// <summary>Окремий ключ сесії для кожної страви (0 — нова страва).</summary>
        private static string DraftKey(int dishId) => $"DishDraft:{dishId}";

        // GET: DishEditor/Create
        public async Task<IActionResult> Create()
        {
            var draft = HttpContext.Session.GetObject<DishDraft>(DraftKey(0));
            if (draft == null)
            {
                draft = new DishDraft();
                HttpContext.Session.SetObject(DraftKey(0), draft);
            }
            else
            {
                ViewData["Restored"] = true;
            }

            await LoadIngredientListAsync(draft);
            return View("Editor", draft);
        }

        // GET: DishEditor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var draft = HttpContext.Session.GetObject<DishDraft>(DraftKey(id));
            if (draft == null)
            {
                // Чернетки ще немає — беремо дані з бази та кладемо в сесію
                var dish = await _context.Dishes
                    .Include(d => d.DishIngredients)
                        .ThenInclude(di => di.Ingredient)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dish == null) return NotFound();

                draft = DishDraft.FromDish(dish);
                HttpContext.Session.SetObject(DraftKey(id), draft);
            }
            else
            {
                ViewData["Restored"] = true;
            }

            await LoadIngredientListAsync(draft);
            return View("Editor", draft);
        }

        // POST: DishEditor/Editor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editor(DishDraft draft, string? command,
            int? newIngredientId, decimal? newQuantity, string? newNote)
        {
            var key = DraftKey(draft.DishId);
            command ??= "";

            if (command == "cancel")
            {
                HttpContext.Session.Remove(key);
                TempData["Success"] = "Зміни скасовано, чернетку видалено із сесії.";
                return draft.DishId == 0
                    ? RedirectToAction("Index", "Dishes")
                    : RedirectToAction("Details", "Dishes", new { id = draft.DishId });
            }

            if (command == "add")
            {
                ModelState.Clear();   // перевіряємо лише новий рядок складу
                await AddItemAsync(draft, newIngredientId, newQuantity, newNote);
            }
            else if (command.StartsWith("remove-") && int.TryParse(command["remove-".Length..], out var index))
            {
                ModelState.Clear();
                if (index >= 0 && index < draft.Items.Count)
                    draft.Items.RemoveAt(index);
            }
            else if (command == "save")
            {
                await ValidateUniqueNameAsync(draft);

                if (ModelState.IsValid)
                {
                    var dishId = await SaveToDatabaseAsync(draft);
                    HttpContext.Session.Remove(key);   // очищення сесії після запису в БД
                    TempData["Success"] = $"Страву «{draft.Name}» збережено в базі даних.";
                    return RedirectToAction("Details", "Dishes", new { id = dishId });
                }
            }

            // Залишаємось на сторінці: поточний стан форми зберігаємо в сесії
            HttpContext.Session.SetObject(key, draft);
            await LoadIngredientListAsync(draft);
            return View("Editor", draft);
        }

        // POST: DishEditor/AutoSave — фонове збереження введених даних у сесію (без запису в БД)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutoSave(DishDraft draft)
        {
            var key = DraftKey(draft.DishId);

            // Оновлюємо лише активну чернетку: після «Зберегти в базу» чи «Скасувати»
            // ключ уже видалено, тож запізнілий запит не створить її знову
            if (HttpContext.Session.GetString(key) == null)
                return NoContent();

            HttpContext.Session.SetObject(key, draft);
            return NoContent();
        }
        private async Task AddItemAsync(DishDraft draft, int? ingredientId, decimal? quantity, string? note)
        {
            if (ingredientId == null)
            {
                ViewData["NewItemError"] = "Оберіть інгредієнт";
                return;
            }
            if (quantity == null || quantity <= 0 || quantity > 1000)
            {
                ViewData["NewItemError"] = "Кількість має бути від 0.001 до 1000";
                return;
            }
            if (draft.Items.Any(i => i.IngredientId == ingredientId))
            {
                ViewData["NewItemError"] = "Цей інгредієнт уже є у складі страви";
                return;
            }

            var ingredient = await _context.Ingredients.FindAsync(ingredientId.Value);
            if (ingredient == null)
            {
                ViewData["NewItemError"] = "Інгредієнт не знайдено";
                return;
            }

            draft.Items.Add(new DraftItem
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                UnitName = ingredient.Unit.GetDisplayName(),
                PricePerUnit = ingredient.PricePerUnit,
                Quantity = quantity.Value,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
            });
        }

        private async Task ValidateUniqueNameAsync(DishDraft draft)
        {
            var name = draft.Name?.Trim() ?? string.Empty;
            if (name.Length > 0 &&
                await _context.Dishes.AnyAsync(d => d.Name == name && d.Id != draft.DishId))
            {
                ModelState.AddModelError(nameof(DishDraft.Name), "Страва з такою назвою вже існує");
            }
        }

        /// <summary>Запис чернетки в БД. Повертає Id страви.</summary>
        private async Task<int> SaveToDatabaseAsync(DishDraft draft)
        {
            Dish dish;
            if (draft.DishId == 0)
            {
                dish = new Dish();
                _context.Dishes.Add(dish);
            }
            else
            {
                dish = await _context.Dishes
                    .Include(d => d.DishIngredients)
                    .FirstAsync(d => d.Id == draft.DishId);
            }

            dish.Name = draft.Name.Trim();
            dish.Description = draft.Description;
            dish.Category = draft.Category;
            dish.Price = draft.Price;
            dish.CookingTimeMinutes = draft.CookingTimeMinutes;
            dish.PortionWeightGrams = draft.PortionWeightGrams;
            dish.IsAvailable = draft.IsAvailable;
            dish.CreatedAt = draft.CreatedAt;

            // Склад: видаляємо прибрані, оновлюємо наявні, додаємо нові
            foreach (var old in dish.DishIngredients.ToList())
            {
                if (!draft.Items.Any(i => i.IngredientId == old.IngredientId))
                    _context.DishIngredients.Remove(old);
            }

            foreach (var item in draft.Items)
            {
                var existing = dish.DishIngredients.FirstOrDefault(di => di.IngredientId == item.IngredientId);
                if (existing == null)
                {
                    dish.DishIngredients.Add(new DishIngredient
                    {
                        IngredientId = item.IngredientId,
                        Quantity = item.Quantity,
                        Note = item.Note
                    });
                }
                else
                {
                    existing.Quantity = item.Quantity;
                    existing.Note = item.Note;
                }
            }

            await _context.SaveChangesAsync();
            return dish.Id;
        }

        /// <summary>Список інгредієнтів, яких ще немає в чернетці.</summary>
        private async Task LoadIngredientListAsync(DishDraft draft)
        {
            var usedIds = draft.Items.Select(i => i.IngredientId).ToList();

            var available = await _context.Ingredients
                .Where(i => !usedIds.Contains(i.Id))
                .OrderBy(i => i.Name)
                .ToListAsync();

            ViewData["Ingredients"] = new SelectList(
                available.Select(i => new
                {
                    i.Id,
                    Title = $"{i.Name} ({i.PricePerUnit:0.00} грн/{i.Unit.GetDisplayName()})"
                }),
                "Id", "Title");
        }
    }
}