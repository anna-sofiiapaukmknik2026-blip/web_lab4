namespace RestaurantKitchen.Models
{
    /// <summary>Дані для головної сторінки (зведена статистика кухні).</summary>
    public class HomeViewModel
    {
        public int DishCount { get; set; }
        public int AvailableDishCount { get; set; }
        public int IngredientCount { get; set; }
        public int AllergenCount { get; set; }
        public List<Ingredient> LowStockIngredients { get; set; } = new();
        public List<Dish> LatestDishes { get; set; } = new();
    }
}