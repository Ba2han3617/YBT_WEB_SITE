using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Data.Context;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount = await _context.Users.CountAsync();
        ViewBag.EventCount = await _context.Events.CountAsync(e => e.IsActive);
        ViewBag.ProjectCount = await _context.Projects.CountAsync(p => p.IsActive);
        ViewBag.BlogCount = await _context.Blogs.CountAsync(b => b.IsActive);
        ViewBag.ApplicationCount = await _context.EventApplications.CountAsync();
        ViewBag.PendingApplicationCount = await _context.EventApplications.CountAsync(a => a.Status == "Yeni Başvuru" || a.Status == "Değerlendiriliyor");

        var recentApplications = await _context.EventApplications
            .Include(ea => ea.Event)
            .Include(ea => ea.User)
            .OrderByDescending(ea => ea.CreatedAt)
            .Take(8)
            .ToListAsync();

        return View(recentApplications);
    }
}
