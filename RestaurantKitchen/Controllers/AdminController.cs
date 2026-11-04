using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Controllers
{
    /// <summary>Адмін-панель: керування користувачами. Доступна лише ролі Admin.</summary>
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin?search=anna
        public async Task<IActionResult> Index(string? search)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Email!.Contains(search) || u.FullName.Contains(search));

            var users = await query.OrderBy(u => u.RegisteredAt).ToListAsync();
            var currentId = _userManager.GetUserId(User);

            var model = new List<UserRowViewModel>();
            foreach (var u in users)
            {
                model.Add(new UserRowViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    RegisteredAt = u.RegisteredAt,
                    IsAdmin = await _userManager.IsInRoleAsync(u, Roles.Admin),
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    IsCurrent = u.Id == currentId
                });
            }

            ViewData["Search"] = search;
            return View(model);
        }

        // POST: Admin/ToggleAdmin — видати / забрати роль адміністратора
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await FindOtherUserAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            {
                await _userManager.RemoveFromRoleAsync(user, Roles.Admin);
                TempData["Success"] = $"У користувача {user.Email} забрано роль адміністратора.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, Roles.Admin);
                TempData["Success"] = $"Користувачу {user.Email} надано роль адміністратора.";
            }

            await _userManager.UpdateSecurityStampAsync(user);   // зміни ролі застосуються після повторного входу
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ToggleLock — заблокувати / розблокувати
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await FindOtherUserAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"Користувача {user.Email} розблоковано.";
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.UpdateSecurityStampAsync(user);   // завершує його активні сеанси
                TempData["Success"] = $"Користувача {user.Email} заблоковано.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await FindOtherUserAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"Користувача {user.Email} видалено.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Знаходить користувача, не дозволяючи адміністратору змінювати самого себе.</summary>
        private async Task<ApplicationUser?> FindOtherUserAsync(string id)
        {
            if (id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "Не можна змінювати власний обліковий запис.";
                return null;
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                TempData["Error"] = "Користувача не знайдено.";
            return user;
        }
    }
}