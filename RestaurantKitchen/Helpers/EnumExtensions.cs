using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RestaurantKitchen.Helpers
{
    public static class EnumExtensions
    {
        /// <summary>Повертає назву з атрибута [Display] для значення enum.</summary>
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
        }
    }
}