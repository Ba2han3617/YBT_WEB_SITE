using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Data.Context;
using Ybt.Service.Services;

namespace Ybt.Web.Controllers;

public class ProjectsController : Controller
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? tech, string? q)
    {
        var query = _context.Projects.Where(p => p.IsActive).AsQueryable();

        // Extract distinct tech tags for filter pills
        var activeProjects = await _context.Projects
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.TechTags))
            .Select(p => p.TechTags!)
            .ToListAsync();

        var techTags = activeProjects
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        ViewBag.TechTags = techTags;
        ViewBag.SelectedTech = tech;
        ViewBag.SearchQuery = q;

        if (!string.IsNullOrWhiteSpace(tech))
        {
            var filterTech = tech.Trim().ToLower();
            query = query.Where(p => p.TechTags != null && p.TechTags.ToLower().Contains(filterTech));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                     p.Description.ToLower().Contains(search) || 
                                     (p.TechTags != null && p.TechTags.ToLower().Contains(search)) ||
                                     (p.TeamMembers != null && p.TeamMembers.ToLower().Contains(search)));
        }

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(projects);
    }
}
