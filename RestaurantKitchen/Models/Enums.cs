using System.ComponentModel.DataAnnotations;

namespace RestaurantKitchen.Models
{
    public enum DishCategory
    {
        [Display(Name = "Закуска")] Appetizer = 1,
        [Display(Name = "Перша страва")] Soup = 2,
        [Display(Name = "Основна страва")] MainCourse = 3,
        [Display(Name = "Десерт")] Dessert = 4,
        [Display(Name = "Напій")] Drink = 5
    }

    public enum MeasureUnit
    {
        [Display(Name = "г")] Gram = 1,
        [Display(Name = "кг")] Kilogram = 2,
        [Display(Name = "мл")] Milliliter = 3,
        [Display(Name = "л")] Liter = 4,
        [Display(Name = "шт")] Piece = 5
    }
}