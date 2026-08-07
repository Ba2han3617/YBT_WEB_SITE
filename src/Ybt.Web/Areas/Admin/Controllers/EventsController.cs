using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly IService<Event> _eventService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EventsController(IService<Event> eventService, IWebHostEnvironment webHostEnvironment)
    {
        _eventService = eventService;
        _webHostEnvironment = webHostEnvironment;
    }


    public async Task<IActionResult> Index()
    {
        var events = await _eventService.GetAllAsync();
        return View(events);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,EventDate,Location,IsActive")] Event @event, IFormFile? imageFile)
    {
        @event.Slug = GenerateSlug(@event.Title);
        @event.EventDate = DateTime.SpecifyKind(@event.EventDate, DateTimeKind.Utc);
        ModelState.Remove("Slug");

        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    @event.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(@event);
                }
            }

            await _eventService.AddAsync(@event);
            TempData["Success"] = "Etkinlik başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        return View(@event);
    }

    private string GenerateSlug(string title)
    {
        if (string.IsNullOrEmpty(title)) return Guid.NewGuid().ToString();
        string slug = title.ToLowerInvariant();
        slug = slug.Replace(" ", "-").Replace("ö", "o").Replace("ü", "u").Replace("ı", "i").Replace("ş", "s").Replace("ç", "c").Replace("ğ", "g");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
        slug = slug.Replace(" ", "-");
        return slug;
    }


    public async Task<IActionResult> Edit(int id)
    {
        var @event = await _eventService.GetByIdAsync(id);
        if (@event == null) return NotFound();
        return View(@event);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,Title,Description,EventDate,Location,ImageUrl,IsActive")] Event @event, IFormFile? imageFile)
    {
        @event.Slug = GenerateSlug(@event.Title);
        @event.EventDate = DateTime.SpecifyKind(@event.EventDate, DateTimeKind.Utc);
        ModelState.Remove("Slug");

        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    @event.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(@event);
                }
            }

            await _eventService.UpdateAsync(@event);
            TempData["Success"] = "Etkinlik başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(@event);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _eventService.RemoveAsync(id);
        TempData["Success"] = "Etkinlik silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("Yüklenecek dosya seçilmedi veya dosya boş.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Yalnızca .jpg, .jpeg, .png ve .webp uzantılı görseller yüklenebilir.");
        }

        if (file.Length > 2 * 1024 * 1024)
        {
            throw new InvalidOperationException("Dosya boyutu 2 MB sınırını aşamaz.");
        }

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return "/uploads/events/" + fileName;
    }
}
