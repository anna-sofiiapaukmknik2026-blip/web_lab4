using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantKitchen.Models
{
    /// <summary>
    /// Склад страви: проміжна таблиця зв'язку "багато-до-багатьох"
    /// між стравами та інгредієнтами з додатковими даними (кількість, примітка).
    /// </summary>
    public class DishIngredient
    {
        [Display(Name = "Страва")]
        public int DishId { get; set; }
        public Dish? Dish { get; set; }

        [Required(ErrorMessage = "Оберіть інгредієнт")]
        [Display(Name = "Інгредієнт")]
        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        [Range(0.001, 1000, ErrorMessage = "Кількість має бути більшою за 0")]
        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Кількість (в одиницях інгредієнта)")]
        public decimal Quantity { get; set; }

        [StringLength(200)]
        [Display(Name = "Примітка")]
        public string? Note { get; set; }

        [NotMapped]
        [Display(Name = "Вартість, грн")]
        public decimal Cost => Ingredient == null ? 0 : Math.Round(Quantity * Ingredient.PricePerUnit, 2);
    }
}