using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Web.Models;
using Microsoft.AspNetCore.Identity;
using Ybt.Core.Entities;
using Ybt.Data.Context;

namespace Ybt.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, AppDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel();

        try
        {
            var now = DateTime.UtcNow;

            vm.EventCount = await _context.Events.CountAsync(e => e.IsActive);
            vm.ActiveProjectCount = await _context.Projects.CountAsync(p => p.IsActive);
            vm.BlogCount = await _context.Blogs.CountAsync(b => b.IsActive);

            vm.UpcomingEvents = await _context.Events
                .Where(e => e.IsActive && e.EventDate >= now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();

            vm.FeaturedProjects = await _context.Projects
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            vm.RecentBlogs = await _context.Blogs
                .Where(b => b.IsActive)
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            vm.TeamMembers = await _context.TeamMembers
                .Where(t => t.IsActive)
                .OrderBy(t => t.Order)
                .Take(6)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ana sayfa verileri yüklenirken hata oluştu.");
            // Sayfa varsayılan boş değerlerle kırılmadan devam eder
        }

        return View(vm);
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
