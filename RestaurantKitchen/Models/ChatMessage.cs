using System.ComponentModel.DataAnnotations;

namespace RestaurantKitchen.Models
{
    /// <summary>Повідомлення чату (зберігається в БД, щоб була історія).</summary>
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        public bool SenderIsAdmin { get; set; }

        /// <summary>Отримувач приватного повідомлення; null — загальний чат.</summary>
        [StringLength(450)]
        public string? RecipientId { get; set; }

        [StringLength(100)]
        public string? RecipientName { get; set; }

        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>Номер замовлення, якого стосується повідомлення (необов'язково).</summary>
        [StringLength(20)]
        public string? OrderNumber { get; set; }

        [StringLength(200)]
        public string? AttachmentUrl { get; set; }

        [StringLength(200)]
        public string? AttachmentName { get; set; }

        public bool IsImage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}