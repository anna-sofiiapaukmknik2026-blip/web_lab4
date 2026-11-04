using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RestaurantKitchen.Models
{
    /// <summary>
    /// Користувач системи. Успадковує стандартні поля Identity
    /// (Id, UserName, Email, PasswordHash тощо) і додає власні.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Ім'я")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Дата реєстрації")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}