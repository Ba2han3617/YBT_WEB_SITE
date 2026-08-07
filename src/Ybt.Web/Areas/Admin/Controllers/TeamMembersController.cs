using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TeamMembersController : Controller
{
    private readonly IService<TeamMember> _teamService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TeamMembersController(IService<TeamMember> teamService, IWebHostEnvironment webHostEnvironment)
    {
        _teamService = teamService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var members = await _teamService.GetAllAsync();
        return View(members.OrderBy(m => m.Order));
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Role,Order,IsActive")] TeamMember member, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    member.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(member);
                }
            }

            await _teamService.AddAsync(member);
            TempData["Success"] = "Ekip üyesi eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(member);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var member = await _teamService.GetByIdAsync(id);
        if (member == null) return NotFound();
        return View(member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,FullName,Role,ImageUrl,Order,IsActive")] TeamMember member, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    member.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(member);
                }
            }

            await _teamService.UpdateAsync(member);
            TempData["Success"] = "Ekip üyesi güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _teamService.RemoveAsync(id);
        TempData["Success"] = "Ekip üyesi silindi.";
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

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "team");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return "/uploads/team/" + fileName;
    }
}
