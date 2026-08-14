using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var userRoles = new Dictionary<int, IList<string>>();

        foreach (var u in users)
        {
            var r = await _userManager.GetRolesAsync(u);
            userRoles[u.Id] = r;
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = roles.ToList();
        ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(int id, string role)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var currentAdmin = await _userManager.GetUserAsync(User);
        if (currentAdmin != null && currentAdmin.Id == user.Id && role != "Admin")
        {
            TempData["Error"] = "Kendi yöneticilik (Admin) rolünüzü kaldıramazsınız!";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = $"{user.FirstName} {user.LastName} kullanıcısının rolü '{role}' olarak güncellendi.";
        }
        else
        {
            TempData["Error"] = "Geçersiz rol seçimi.";
        }

        return RedirectToAction(nameof(Details), new { id = user.Id });
    }
}
