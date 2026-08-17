using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Data.Context;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class EventsController : Controller
{
    private readonly IService<Event> _eventService;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public EventsController(IService<Event> eventService, UserManager<AppUser> userManager, AppDbContext context)
    {
        _eventService = eventService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index(string? category, string? q)
    {
        var query = _context.Events.Where(e => e.IsActive).AsQueryable();

        // Get distinct active categories for filter tabs
        var categories = await _context.Events
            .Where(e => e.IsActive && !string.IsNullOrEmpty(e.Category))
            .Select(e => e.Category!)
            .Distinct()
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = category;
        ViewBag.SearchQuery = q;

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(e => e.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(search) || 
                                     e.Description.ToLower().Contains(search) || 
                                     e.Location.ToLower().Contains(search) ||
                                     (e.Speaker != null && e.Speaker.ToLower().Contains(search)));
        }

        var events = await query
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return View(events);
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var @event = await _context.Events
            .Include(e => e.Applications)
            .FirstOrDefaultAsync(e => e.Slug == slug && e.IsActive);

        if (@event == null) return NotFound();

        ViewBag.ApplicationsCount = @event.Applications.Count;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userApplication = await _context.EventApplications
                    .FirstOrDefaultAsync(ea => ea.EventId == @event.Id && ea.UserId == user.Id);
                ViewBag.UserApplication = userApplication;
            }
        }

        return View(@event);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int eventId, string? notes)
    {
        var @event = await _context.Events.FindAsync(eventId);
        if (@event == null || !@event.IsActive)
        {
            TempData["Error"] = "Başvurulmak istenen etkinlik bulunamadı veya yayınlanmamış.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var alreadyApplied = await _context.EventApplications
            .AnyAsync(ea => ea.EventId == eventId && ea.UserId == user.Id);

        if (alreadyApplied)
        {
            TempData["Error"] = "Bu etkinliğe daha önce başvuruda bulundunuz.";
            return RedirectToAction(nameof(Details), new { slug = @event.Slug });
        }

        var application = new EventApplication
        {
            EventId = eventId,
            UserId = user.Id,
            Status = "Yeni Başvuru",
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EventApplications.AddAsync(application);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"'{ @event.Title }' etkinliğine başvurunuz başarıyla alındı!";
        return RedirectToAction(nameof(Details), new { slug = @event.Slug });
    }
}
