using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ybt.Web.Models;
using Microsoft.AspNetCore.Identity;
using Ybt.Core.Entities;

namespace Ybt.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> TestAdmin()
    {
        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            return Content("Admin user not found");
        }

        var roles = await _userManager.GetRolesAsync(adminUser);
        var isInRole = await _userManager.IsInRoleAsync(adminUser, "Admin");

        return Content($"Admin user: {adminUser.UserName}, Email: {adminUser.Email}, Roles: {string.Join(", ", roles)}, IsInAdminRole: {isInRole}");
    }
}
