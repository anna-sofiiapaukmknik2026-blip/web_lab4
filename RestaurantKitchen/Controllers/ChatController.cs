using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Data;
using RestaurantKitchen.Helpers;
using RestaurantKitchen.Hubs;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>Сторінка чату, завантаження файлів і очищення історії.</summary>
    [Authorize]
    public class ChatController : Controller
    {
        private readonly KitchenContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(KitchenContext context, UserManager<ApplicationUser> userManager,
                              IWebHostEnvironment env, IHubContext<ChatHub> hub)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _hub = hub;
        }

        // GET: Chat
        public async Task<IActionResult> Index()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var isAdmin = await _userManager.IsInRoleAsync(me, Roles.Admin);
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
            var adminIds = admins.Select(a => a.Id).ToHashSet();

            // Історія: загальні повідомлення + особисті, де я відправник або отримувач
            var messages = await _context.ChatMessages
                .Where(m => m.RecipientId == null || m.RecipientId == me.Id || m.SenderId == me.Id)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();
            messages.Reverse();

            // Адміністратор може писати особисто будь-кому, клієнт — лише адміністраторам
            var candidates = isAdmin
                ? await _userManager.Users.Where(u => u.Id != me.Id).OrderBy(u => u.FullName).ToListAsync()
                : admins.Where(a => a.Id != me.Id).ToList();

            var model = new ChatPageViewModel
            {
                CurrentUserId = me.Id,
                IsAdmin = isAdmin,
                Messages = messages.Select(ChatMessageView.From).ToList(),
                Recipients = candidates.Select(u => new ChatRecipient
                {
                    Id = u.Id,
                    Name = ChatHub.DisplayName(u),
                    IsAdmin = adminIds.Contains(u.Id)
                }).ToList()
            };

            return View(model);
        }

        // POST: Chat/Upload — зберігає файл і повертає його адресу (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Файл не вибрано" });

            if (file.Length > ChatFiles.MaxFileSize)
                return BadRequest(new { error = "Файл завеликий (максимум 5 МБ)" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ChatFiles.AllowedExtensions.Contains(ext))
                return BadRequest(new { error = "Недозволений тип файлу. Дозволено: зображення, pdf, txt, docx, xlsx" });

            var folder = Path.Combine(_env.WebRootPath, "uploads", "chat");
            Directory.CreateDirectory(folder);

            // Випадкова назва: не можна перезаписати чужий файл чи вгадати адресу
            var storedName = $"{Guid.NewGuid():N}{ext}";
            await using (var stream = System.IO.File.Create(Path.Combine(folder, storedName)))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new
            {
                url = $"/uploads/chat/{storedName}",
                name = Path.GetFileName(file.FileName),
                isImage = ChatFiles.ImageExtensions.Contains(ext)
            });
        }

        // POST: Chat/ClearHistory — лише для адміністратора
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ClearHistory()
        {
            // Видаляємо файли, прикріплені до повідомлень
            var files = await _context.ChatMessages
                .Where(m => m.AttachmentUrl != null)
                .Select(m => m.AttachmentUrl!)
                .ToListAsync();

            foreach (var url in files)
            {
                var path = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            // Видаляємо всі повідомлення однією командою SQL
            var count = await _context.ChatMessages.ExecuteDeleteAsync();

            // Повідомляємо всі відкриті вікна чату, що історію очищено
            await _hub.Clients.All.SendAsync("HistoryCleared");

            TempData["Success"] = $"Історію чату очищено (видалено повідомлень: {count}).";
            return RedirectToAction(nameof(Index));
        }
    }
}