using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantKitchen.Models
{
    /// <summary>Страва меню ресторану.</summary>
    public class Dish
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть назву страви")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Display(Name = "Категорія")]
        public DishCategory Category { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Ціна має бути більшою за 0")]
        [Column(TypeName = "decimal(10,2)")]
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

        public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();

        /// <summary>Собівартість страви за складом (не зберігається в БД).</summary>
        [NotMapped]
        [Display(Name = "Собівартість, грн")]
        public decimal FoodCost => DishIngredients.Sum(di => di.Cost);
    }
}