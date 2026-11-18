namespace RestaurantKitchen.Models
{
    /// <summary>Дані, які браузер надсилає в хаб.</summary>
    public class ChatMessageInput
    {
        public string? Text { get; set; }
        public string? RecipientId { get; set; }
        public string? OrderNumber { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
    }

    /// <summary>Повідомлення у вигляді для відображення в браузері.</summary>
    public class ChatMessageView
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public bool SenderIsAdmin { get; set; }
        public string? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public bool IsPrivate { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public bool IsImage { get; set; }
        public string SentAt { get; set; } = string.Empty;

        public static ChatMessageView From(ChatMessage m) => new()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.SenderName,
            SenderIsAdmin = m.SenderIsAdmin,
            RecipientId = m.RecipientId,
            RecipientName = m.RecipientName,
            IsPrivate = m.RecipientId != null,
            Text = m.Text,
            OrderNumber = m.OrderNumber,
            AttachmentUrl = m.AttachmentUrl,
            AttachmentName = m.AttachmentName,
            IsImage = m.IsImage,
            SentAt = m.SentAt.ToString("dd.MM HH:mm")
        };
    }

    /// <summary>Можливий отримувач особистого повідомлення.</summary>
    public class ChatRecipient
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    /// <summary>Дані сторінки чату.</summary>
    public class ChatPageViewModel
    {
        public string CurrentUserId { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public List<ChatMessageView> Messages { get; set; } = new();
        public List<ChatRecipient> Recipients { get; set; } = new();
    }
}