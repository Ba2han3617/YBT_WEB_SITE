using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BlogsController : Controller
{
    private readonly IService<Blog> _blogService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BlogsController(IService<Blog> blogService, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _blogService = blogService;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var blogs = await _blogService.GetAllAsync();
        return View(blogs);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Content,IsActive")] Blog blog, IFormFile? imageFile)
    {
        blog.Slug = GenerateSlug(blog.Title);
        ModelState.Remove("Slug");
        ModelState.Remove("Author");

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            blog.AuthorId = user.Id;
        }

        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    blog.ImageUrl = await SaveImage(imageFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(blog);
                }
            }

            await _blogService.AddAsync(blog);
            TempData["Success"] = "Blog yazısı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        return View(blog);
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

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "blogs");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return "/uploads/blogs/" + fileName;
    }
}
