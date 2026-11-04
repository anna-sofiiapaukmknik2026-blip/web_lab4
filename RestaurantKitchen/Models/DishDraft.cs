using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RestaurantKitchen.Helpers;

namespace RestaurantKitchen.Models
{
    /// <summary>
    /// Чернетка страви, яка зберігається в сесії під час створення або редагування.
    /// </summary>
    public class DishDraft : IValidatableObject
    {
        /// <summary>Id страви в БД; 0 — нова страва.</summary>
        public int DishId { get; set; }

        [Required(ErrorMessage = "Вкажіть назву страви")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Опис не може бути довшим за 500 символів")]
        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        [Display(Name = "Категорія")]
        public DishCategory Category { get; set; } = DishCategory.MainCourse;

        [Range(0.01, 100000, ErrorMessage = "Ціна має бути від 0.01 до 100000 грн")]
        [Display(Name = "Ціна, грн")]
        public decimal Price { get; set; }

        [Range(1, 600, ErrorMessage = "Час приготування від 1 до 600 хв")]
        [Display(Name = "Час приготування, хв")]
        public int CookingTimeMinutes { get; set; }

        [Range(1, 5000, ErrorMessage = "Вихід порції від 1 до 5000 г")]
        [Display(Name = "Вихід порції, г")]
        public int PortionWeightGrams { get; set; }

        [Display(Name = "Є в меню")]
        public bool IsAvailable { get; set; } = true;

        [DataType(DataType.Date)]
        [Display(Name = "Додано до меню")]
        public DateTime CreatedAt { get; set; } = DateTime.Today;

        public List<DraftItem> Items { get; set; } = new();

        [JsonIgnore]
        public decimal FoodCost => Items.Sum(i => i.Cost);

        /// <summary>Перевірки, що стосуються кількох полів одразу (лише на сервері).</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CreatedAt.Date > DateTime.Today)
                yield return new ValidationResult(
                    "Дата додавання до меню не може бути в майбутньому",
                    new[] { nameof(CreatedAt) });

            if (Items.Count > 0 && Price < FoodCost)
                yield return new ValidationResult(
                    $"Ціна ({Price:0.00} грн) не може бути меншою за собівартість ({FoodCost:0.00} грн)",
                    new[] { nameof(Price) });
        }

        /// <summary>Створює чернетку з даних, отриманих з бази.</summary>
        public static DishDraft FromDish(Dish dish) => new()
        {
            DishId = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Category = dish.Category,
            Price = dish.Price,
            CookingTimeMinutes = dish.CookingTimeMinutes,
            PortionWeightGrams = dish.PortionWeightGrams,
            IsAvailable = dish.IsAvailable,
            CreatedAt = dish.CreatedAt,
            Items = dish.DishIngredients
                .OrderBy(di => di.Ingredient!.Name)
                .Select(di => new DraftItem
                {
                    IngredientId = di.IngredientId,
                    IngredientName = di.Ingredient!.Name,
                    UnitName = di.Ingredient.Unit.GetDisplayName(),
                    PricePerUnit = di.Ingredient.PricePerUnit,
                    Quantity = di.Quantity,
                    Note = di.Note
                })
                .ToList()
        };
    }

    /// <summary>Рядок складу страви в чернетці.</summary>
    public class DraftItem
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }

        [Range(0.001, 1000, ErrorMessage = "Кількість має бути від 0.001 до 1000")]
        [Display(Name = "Кількість")]
        public decimal Quantity { get; set; }

        [StringLength(200, ErrorMessage = "Примітка не довша за 200 символів")]
        [Display(Name = "Примітка")]
        public string? Note { get; set; }

        [JsonIgnore]
        public decimal Cost => Math.Round(Quantity * PricePerUnit, 2);
    }
}