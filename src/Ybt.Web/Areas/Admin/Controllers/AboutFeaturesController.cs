using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AboutFeaturesController : Controller
{
    private readonly IService<AboutFeature> _featureService;

    public AboutFeaturesController(IService<AboutFeature> featureService)
    {
        _featureService = featureService;
    }

    public async Task<IActionResult> Index()
    {
        var features = await _featureService.GetAllAsync();
        return View(features.OrderBy(f => f.Order));
    }

    public IActionResult Create()
    {
        var model = new AboutFeature
        {
            Order = 1,
            IsActive = true,
            AccentType = "cyan",
            Icon = "bi-code-slash"
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Icon,Tags,AccentType,Order,IsActive")] AboutFeature feature)
    {
        if (string.IsNullOrWhiteSpace(feature.Title))
        {
            ModelState.AddModelError("Title", "Başlık alanı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(feature.Description))
        {
            ModelState.AddModelError("Description", "Açıklama alanı zorunludur.");
        }

        if (ModelState.IsValid)
        {
            feature.Icon = string.IsNullOrWhiteSpace(feature.Icon) ? "bi-code-slash" : feature.Icon.Trim();
            feature.AccentType = string.IsNullOrWhiteSpace(feature.AccentType) ? "cyan" : feature.AccentType.Trim().ToLower();

            await _featureService.AddAsync(feature);
            TempData["Success"] = "Neler Yapıyoruz kartı başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        return View(feature);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var feature = await _featureService.GetByIdAsync(id);
        if (feature == null) return NotFound();
        return View(feature);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Icon,Tags,AccentType,Order,IsActive")] AboutFeature feature)
    {
        if (id != feature.Id) return NotFound();

        if (string.IsNullOrWhiteSpace(feature.Title))
        {
            ModelState.AddModelError("Title", "Başlık alanı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(feature.Description))
        {
            ModelState.AddModelError("Description", "Açıklama alanı zorunludur.");
        }

        if (ModelState.IsValid)
        {
            var existing = await _featureService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = feature.Title;
            existing.Description = feature.Description;
            existing.Icon = string.IsNullOrWhiteSpace(feature.Icon) ? "bi-code-slash" : feature.Icon.Trim();
            existing.Tags = feature.Tags;
            existing.AccentType = string.IsNullOrWhiteSpace(feature.AccentType) ? "cyan" : feature.AccentType.Trim().ToLower();
            existing.Order = feature.Order;
            existing.IsActive = feature.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _featureService.UpdateAsync(existing);
            TempData["Success"] = "Kart bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        return View(feature);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var feature = await _featureService.GetByIdAsync(id);
        if (feature != null)
        {
            await _featureService.RemoveAsync(id);
            TempData["Success"] = "Kart başarıyla silindi.";
        }
        else
        {
            TempData["Error"] = "Silinmek istenen kart bulunamadı.";
        }

        return RedirectToAction(nameof(Index));
    }
}
