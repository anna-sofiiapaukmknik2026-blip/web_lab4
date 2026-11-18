using System.Text.RegularExpressions;

namespace RestaurantKitchen.Helpers
{
    /// <summary>Обмеження для файлів, що передаються в чаті.</summary>
    public static class ChatFiles
    {
        public const long MaxFileSize = 5 * 1024 * 1024;   // 5 МБ

        public static readonly HashSet<string> ImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".txt", ".docx", ".xlsx" };

        // Файли зберігаються під випадковою назвою: /uploads/chat/<32 символи>.<розширення>
        private static readonly Regex StoredUrl = new(@"^/uploads/chat/[0-9a-f]{32}\.[a-z0-9]{2,5}$");

        public static bool IsValidUrl(string url) => StoredUrl.IsMatch(url);

        public static bool IsImage(string url) => ImageExtensions.Contains(Path.GetExtension(url));
    }
}