using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Data.Context;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ApplicationsController : Controller
{
    private readonly AppDbContext _context;

    public ApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status)
    {
        var allApps = await _context.EventApplications
            .Include(ea => ea.Event)
            .Include(ea => ea.User)
            .OrderByDescending(ea => ea.CreatedAt)
            .ToListAsync();

        ViewBag.TotalCount = allApps.Count;
        ViewBag.PendingCount = allApps.Count(a => a.Status == "Yeni Başvuru" || a.Status == "Pending");
        ViewBag.ReviewingCount = allApps.Count(a => a.Status == "Değerlendiriliyor" || a.Status == "UnderReview");
        ViewBag.ApprovedCount = allApps.Count(a => a.Status == "Onaylandı" || a.Status == "Approved");
        ViewBag.RejectedCount = allApps.Count(a => a.Status == "Reddedildi" || a.Status == "Rejected");
        ViewBag.CurrentFilter = status ?? "all";

        var filtered = status switch
        {
            "pending" => allApps.Where(a => a.Status == "Yeni Başvuru" || a.Status == "Pending"),
            "reviewing" => allApps.Where(a => a.Status == "Değerlendiriliyor" || a.Status == "UnderReview"),
            "approved" => allApps.Where(a => a.Status == "Onaylandı" || a.Status == "Approved"),
            "rejected" => allApps.Where(a => a.Status == "Reddedildi" || a.Status == "Rejected"),
            _ => allApps.AsEnumerable()
        };

        return View(filtered.ToList());
    }

    [HttpGet]
    public async Task<IActionResult> GetDetails(int id)
    {
        var app = await _context.EventApplications
            .Include(ea => ea.Event)
            .Include(ea => ea.User)
            .FirstOrDefaultAsync(ea => ea.Id == id);

        if (app == null) return NotFound();

        return Json(new
        {
            id = app.Id,
            fullName = app.User?.FullName ?? (app.User?.FirstName + " " + app.User?.LastName),
            email = app.User?.Email ?? "Belirtilmemiş",
            phone = app.User?.PhoneNumber ?? "Belirtilmemiş",
            studentNumber = app.User?.StudentNumber ?? "Belirtilmemiş",
            faculty = app.User?.Faculty ?? "Belirtilmemiş",
            department = app.User?.Department ?? "Belirtilmemiş",
            grade = app.User?.Grade ?? "Belirtilmemiş",
            interests = app.User?.Interests ?? "Belirtilmemiş",
            eventTitle = app.Event?.Title ?? "Silinmiş Etkinlik",
            eventDate = app.Event?.EventDate.ToString("dd MMMM yyyy HH:mm") ?? "-",
            eventLocation = app.Event?.Location ?? "-",
            status = app.Status,
            notes = string.IsNullOrEmpty(app.Notes) ? "Kullanıcı başvuru notu bırakmadı." : app.Notes,
            adminNotes = app.AdminNotes ?? "",
            createdAt = app.CreatedAt.ToString("dd.MM.yyyy HH:mm")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNotes, string? returnUrl)
    {
        var app = await _context.EventApplications.FindAsync(id);
        if (app == null)
        {
            TempData["Error"] = "Başvuru kaydı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        app.Status = status;
        app.AdminNotes = adminNotes;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Başvuru durumu '{status}' olarak güncellendi.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
