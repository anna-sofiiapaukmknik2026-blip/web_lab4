using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using RestaurantKitchen.Data;
using RestaurantKitchen.Helpers;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Hubs
{
    /// <summary>
    /// Хаб чату клієнтів з адміністратором щодо замовлень.
    /// Підтримує загальні та приватні повідомлення і вкладені файли.
    /// </summary>
    [Authorize]   // підключатися можуть лише зареєстровані користувачі
    public class ChatHub : Hub
    {
        private readonly KitchenContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(KitchenContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>Викликається з браузера: connection.invoke("SendMessage", {...})</summary>
        public async Task SendMessage(ChatMessageInput input)
        {
            var sender = await _userManager.GetUserAsync(Context.User!)
                         ?? throw new HubException("Користувача не знайдено");
            var senderIsAdmin = await _userManager.IsInRoleAsync(sender, Roles.Admin);

            var text = input.Text?.Trim() ?? string.Empty;
            var orderNumber = string.IsNullOrWhiteSpace(input.OrderNumber) ? null : input.OrderNumber.Trim();
            var hasAttachment = !string.IsNullOrEmpty(input.AttachmentUrl);

            // ---- Перевірка даних ----
            if (text.Length == 0 && !hasAttachment)
                throw new HubException("Введіть текст повідомлення або додайте файл");
            if (text.Length > 2000)
                throw new HubException("Повідомлення не може бути довшим за 2000 символів");
            if (orderNumber != null && orderNumber.Length > 20)
                throw new HubException("Номер замовлення не може бути довшим за 20 символів");
            if (hasAttachment && !ChatFiles.IsValidUrl(input.AttachmentUrl!))
                throw new HubException("Некоректний файл");

            // ---- Отримувач приватного повідомлення ----
            ApplicationUser? recipient = null;
            if (!string.IsNullOrEmpty(input.RecipientId))
            {
                recipient = await _userManager.FindByIdAsync(input.RecipientId)
                            ?? throw new HubException("Отримувача не знайдено");

                if (recipient.Id == sender.Id)
                    throw new HubException("Не можна надіслати повідомлення самому собі");

                // Клієнти пишуть особисто лише адміністраторам
                if (!senderIsAdmin && !await _userManager.IsInRoleAsync(recipient, Roles.Admin))
                    throw new HubException("Особисті повідомлення можна надсилати лише адміністратору");
            }

            // ---- Збереження в БД ----
            var message = new ChatMessage
            {
                SenderId = sender.Id,
                SenderName = Cut(DisplayName(sender), 100),
                SenderIsAdmin = senderIsAdmin,
                RecipientId = recipient?.Id,
                RecipientName = recipient == null ? null : Cut(DisplayName(recipient), 100),
                Text = text,
                OrderNumber = orderNumber,
                AttachmentUrl = hasAttachment ? input.AttachmentUrl : null,
                AttachmentName = hasAttachment ? Cut(input.AttachmentName ?? "файл", 200) : null,
                IsImage = hasAttachment && ChatFiles.IsImage(input.AttachmentUrl!),
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // ---- Розсилка ----
            var view = ChatMessageView.From(message);
            if (recipient == null)
                await Clients.All.SendAsync("ReceiveMessage", view);                                   // загальне
            else
                await Clients.Users(new[] { sender.Id, recipient.Id }).SendAsync("ReceiveMessage", view); // приватне
        }

        public static string DisplayName(ApplicationUser user) =>
            string.IsNullOrWhiteSpace(user.FullName) ? (user.Email ?? user.UserName ?? "") : user.FullName;

        private static string Cut(string value, int max) => value.Length > max ? value[..max] : value;
    }
}