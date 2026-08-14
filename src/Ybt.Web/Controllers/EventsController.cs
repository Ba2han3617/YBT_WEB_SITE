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

    public async Task<IActionResult> Index()
    {
        var events = await _eventService.GetAllAsync();
        return View(events);
    }

    public async Task<IActionResult> Details(string slug)
    {
        var @event = await _context.Events
            .Include(e => e.Applications)
            .FirstOrDefaultAsync(e => e.Slug == slug);

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
            TempData["Error"] = "Başvurulmak istenen etkinlik bulunamadı veya aktif değil.";
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

        TempData["Success"] = $"'{ @event.Title }' etkinliğine başvurunuz başarıyla alındı! Başvurunuzun durumunu aşağıdan takip edebilirsiniz.";
        return RedirectToAction("Applications", "Account");
    }
}
