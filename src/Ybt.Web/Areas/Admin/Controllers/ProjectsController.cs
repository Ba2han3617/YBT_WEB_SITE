using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProjectsController : Controller
{
    private readonly IService<Project> _projectService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProjectsController(IService<Project> projectService, IWebHostEnvironment webHostEnvironment)
    {
        _projectService = projectService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var projects = await _projectService.GetAllAsync();
        return View(projects);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,GitHubUrl,TechTags,IsActive")] Project project, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    project.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(project);
                }
            }

            await _projectService.AddAsync(project);
            TempData["Success"] = "Proje başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,Name,Description,GitHubUrl,TechTags,ImageUrl,IsActive")] Project project, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    project.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(project);
                }
            }

            await _projectService.UpdateAsync(project);
            TempData["Success"] = "Proje başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _projectService.RemoveAsync(id);
        TempData["Success"] = "Proje silindi.";
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

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "projects");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return "/uploads/projects/" + fileName;
    }
}
