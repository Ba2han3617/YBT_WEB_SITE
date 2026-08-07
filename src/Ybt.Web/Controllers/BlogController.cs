using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class BlogController : Controller
{
    private readonly IService<Blog> _blogService;

    public BlogController(IService<Blog> blogService)
    {
        _blogService = blogService;
    }

    public async Task<IActionResult> Index()
    {
        var blogs = await _blogService.GetAllAsync();
        return View(blogs);
    }

    public async Task<IActionResult> Details(string slug)
    {
        var blogs = await _blogService.GetAllAsync();
        var blog = blogs.FirstOrDefault(b => b.Slug == slug);
        if (blog == null) return NotFound();
        return View(blog);
    }
}
