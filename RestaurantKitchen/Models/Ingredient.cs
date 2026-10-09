using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantKitchen.Models
{
    /// <summary>Інгредієнт (продукт) на складі кухні.</summary>
    public class Ingredient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть назву інгредієнта")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 80 символів")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Одиниця виміру")]
        public MeasureUnit Unit { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Ціна має бути більшою за 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ціна за одиницю, грн")]
        public decimal PricePerUnit { get; set; }

        [Range(0, 100000, ErrorMessage = "Залишок не може бути від'ємним")]
        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Залишок на складі")]
        public decimal StockQuantity { get; set; }

        [Range(0, 1000, ErrorMessage = "Калорійність від 0 до 1000 ккал")]
        [Display(Name = "Калорійність, ккал/100 г")]
        public int? CaloriesPer100g { get; set; }

        [StringLength(100)]
        [Display(Name = "Постачальник")]
        public string? Supplier { get; set; }

        [Display(Name = "Алерген")]
        public bool IsAllergen { get; set; }

        public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
    }
}